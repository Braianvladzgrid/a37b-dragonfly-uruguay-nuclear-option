# Development validation

Native Nuclear Option 0.34.2, Unity 2022.3.62f2, Blueprinter 2.0.1.
See References/FLIGHT-CHECKPOINT.md for dated results and pending checks.

Verified: source provenance and GPL notices, 31 converted models, own catalog/map
silhouette, tier 1 and $13m price, two 12680 N dry engines, 2817 kg empty mass.
Builds 12/15/17 completed takeoff and a 100 m climb using native flight forces.
Build 15 completed approach, touchdown and full stop with all 37 parts at 100%.
Later rolling past the runway caused terrain impact; it was a separate fixture
failure, not a damage-free completion of that later taxi check.
Build 17 internal minigun consumed 77 rounds in three seconds, gear-up safety
released normally, all airframe parts remained at 100%. Gear-down safety worked.
Gear retract/extend and native camera switching were exercised. No assertion of
controller orbit sensitivity or multiplayer compatibility is made.

Aircraft uses original suspension/damage, explicit mass stations, source-derived
fixed-wing force points and source gear/door animation. No immunity, ignored
self-collisions, velocity clamp or artificial strengthening is used.

Test fixture: Tests/A37TestProbe.cs temporarily supplies control inputs, logs native
telemetry and places all rigidbodies on a measured pavement/initial approach.
Only initial placement and approach speed are imposed; later forces/collisions
are native. The test DLL must never ship or remain enabled for normal play.
Performance results with the aerobrake incorrectly left open are invalid as
maximum-speed measurements. Final performance, external weapons and final-build
landing acceptance remain pending.
