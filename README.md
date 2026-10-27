# A-37B Dragonfly - Uruguay

Work in progress: tier 1, $13,000,000. An experimental aircraft bundle now
compiles and appears in the game's hangar. Flight acceptance is not complete;
this repository is a development backup, not a finished release.

The pinned FlightGear source includes the aircraft, cockpit, instruments, seats
and pilots. SourceAssets retains the original archive, license and SHA-256
manifest. Assets/Blueprinter/Mods/A37B/Models contains 31 converted OBJ/MTL
models. See CREDITS.md.

Integration uses the native T/A-30's two turbojets, suspension, damage and control
systems, with A-37 geometry and a dedicated silhouette. The intended dry thrust
is 12,680 N per engine, empty mass 2,817 kg and fuel capacity 1,536 kg. These are
configuration targets, not measured flight results. FAU 282 vector markings, a telemetry bridge for the original analog instruments
and a native-system 7.62 mm minigun are integrated. Gun firing and the latest
performance checks remain pending; see References/FLIGHT-CHECKPOINT.md.

Unity 2022.3.62f2 and Blueprinter 2.0.1 are used. Import the installed Nuclear
Option 0.34.2 game through Blueprinter to generate local placeholders and game
assemblies, which are excluded from Git. Run A37VisualBuilder.PrepareFlight in
batch mode without -quit: it synchronizes the game assembly before resuming the
build after a domain reload. RuntimeSupport/compile-runtime.ps1 builds the small
visual/physics support plugin against the installed game. Tests/A37TestProbe.cs
is a local test fixture and must never be distributed in the playable addon.

Acceptance checks will use a noncombat scenario with normal damage and physics.
Do not classify post-impact self contacts as spontaneous structural failure.
Public release requires source/texture attribution, a new spawn, takeoff,
gear/camera cycling, power changes, weapon firing and a landing/brake check.
