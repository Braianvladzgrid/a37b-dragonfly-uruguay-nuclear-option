# A-37 checkpoint - 2026-10-27 00:24 Uruguay

Build 10 compiles successfully. Current game has bundle build 10 and the analog
instrument runtime installed. Flight validation remains incomplete.

Verified build 9 new spawn: all three landing gear contacts true, all 37 parts at
100 health, stationary, loaded mass about 3276 kg at 30 percent fuel. Loaded CG
z about 0.89 m, ahead of main gear z 0.66 m. Dry mass target 2817 kg, dry CG z
1.05 m is an engineering fit to gear geometry, not a real-aircraft datum.

The old place_runway fixture moved only AeroPart rigidbodies. It omitted native
suspension rigidbodies. A nose OnJointBreak occurred one second after relocation,
before throttle was applied. The following acceleration attempt is INVALID for
flight acceptance because the airframe was already mechanically compromised.
Fixture now includes descendant suspension rigidbodies and root transform, and
aborts at the first health reduction. Must verify a fresh spawn and relocation.
No native damage/strength/contact bypass has been introduced.

Build 10 clears incompatible five-station trainer default loadouts to four paired
external station selections. It adds source-calibrated analog gauge markers and
runtime telemetry bridge, pending live visual validation. FAU mesh markings did
not generate in build 10; editor now uses deterministic triangle projection
instead of temporary MeshCollider queries, but that change is NOT built yet.

Public source backup exists on GitHub. No playable release or NOMM submission
should be made until runway takeoff, stable flight, speed, weapons, gear/camera
cycling, landing and manual braking pass. Internal 7.62 mm gun still pending.
Mirage 2000 follows only after A-37 completion, as requested by the user.
