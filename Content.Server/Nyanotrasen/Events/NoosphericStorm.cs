using Content.Server.Station.Systems;
using Content.Server.Abilities.Psionics;
using Content.Server.MobState;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.StationEvents.Events;
/// <summary>
/// Infects a couple people
/// with a random disease that isn't super deadly
/// </summary>
public sealed class NoosphericStorm : StationEventSystem
{
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override string Prototype => "NoosphericStorm";

    /// <summary>
    /// Finds 2-5 random, alive entities that can host diseases
    /// and gives them a randomly selected disease.
    /// They all get the same disease.
    /// </summary>
    public override void Started()
    {
        base.Started();
        HashSet<EntityUid> stationsToNotify = new();
        List<PotentialPsionicComponent> validList = new();
        foreach (var psi in EntityManager.EntityQuery<PotentialPsionicComponent>())
        {
            if (_mobStateSystem.IsDead(psi.Owner))
                continue;

            if (HasComp<PsionicComponent>(psi.Owner) || HasComp<PsionicInsulationComponent>(psi.Owner))
                continue;

            validList.Add(psi);
        }
        RobustRandom.Shuffle(validList);

        var toAwaken = RobustRandom.Next(1, 3);

        // Now we give it to people in the list of living disease carriers earlier
        foreach (var target in validList)
        {
            if (toAwaken-- == 0)
                break;

            _psionicAbilitiesSystem.AddPsionics(target.Owner);

            var station = StationSystem.GetOwningStation(target.Owner);
            if(station == null) continue;
            stationsToNotify.Add((EntityUid) station);
        }
    }
}