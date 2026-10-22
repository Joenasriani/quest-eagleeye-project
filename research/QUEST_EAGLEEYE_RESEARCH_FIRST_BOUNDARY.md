# QUEST-EAGLEEYE: Research-first boundary after Phase 13

**Classification:** technical feasibility/concept study with supporting synthetic software experiment. **Phase 0 verdict:** DEVELOP FURTHER. **Publication readiness:** NOT READY.

## Research mission

Evaluate feasibility and limitations of combining Meta Quest 3 XR, external thermal imaging (TC001 class), LoRa/Meshtastic with external position source, wearable power/data and head-directed interaction. Do not build a finished Unity application by default.

## Implemented evidence

Phase 11 GitHub Actions https://github.com/Joenasriani/quest-eagleeye-project/actions/runs/39765420722 reports 8/8 actual .NET 8 compiled synthetic protocol/receiver tests PASS. This is **not** Quest, TC001, Unity Editor or real-world thermal overlay testing.

## Evidence and claim boundaries

- Quest 3 does not have native eye tracking. Head-directed rays are distinct from eye gaze.
- Passthrough RGB access is distinct from acquisition of external thermal sensor data.
- Heltec WiFi LoRa 32 V3 has no onboard GNSS.
- Thermal camera overlay as a 2D panel is distinct from calibrated world registration.
- Integration performance (latency, power, range, registration, USB host/PD support) remains unmeasured.

## Research sequence

1. Identify prior art in thermal AR, head-mounted display image registration, time synchronization, LoRa and wearable power integration; record real DOI/links and exact comparable mechanisms.
2. Produce evidence-ranked architecture and compatibility matrix; no claim of source-level code compatibility without an executed hardware interface.
3. Calculate input-limited engineering budgets with labelled assumptions: TC001 nominal 256×192 at 25 Hz, and an **assumed** 16-bit-per-pixel transport yields 2,457,600 payload bytes/s, not a measured sensor stream.
4. Define independent experiments: actual TC001 capture and frame format; Quest native capture/display; calibrated thermal-to-world transforms with error distributions; synchronized end-to-end latency; hub/power testing; external GNSS and LoRa link results.
5. Develop a bounded technical report distinguishing already established work from adaptation, hypothesis and genuine new contributions.

## Minimum versus optional software

- Retain and cite the existing synthetic .NET transport receiver as a narrow software feasibility instrument.
- Unity PlayMode, Android IL2CPP APK, polished app, store release and UI design are optional until a specific research question requires them.
- No Quest or TOPDON hardware test has been executed.
- Do not merge to main, publish to Zenodo, or describe the project as an integrated validated XR product.

## Publication value

Potential format: technical feasibility report / engineering concept study; **novel peer-reviewed experimental contribution not established**. Seek publication only if engineering comparisons, citation provenance, source rights and claims are complete. DOI is not validation.

Sources (verified by current research check): 
- Meta Passthrough Camera API https://developers.meta.com/vr/documentation/spatial-sdk/spatial-sdk-pca-overview/
- Meta Quest eye tracking https://developers.meta.com/vr/essentials/horizon-os-input/
- TOPDON TC001 https://www.topdon.us/products/tc001
- Heltec V3 https://heltec.org/project/wifi-lora-32-v3/
- Chegini et al. 2023 https://doi.org/10.1016/j.bjoms.2022.08.007
- Unger et al. 2024 https://pubmed.ncbi.nlm.nih.gov/38789883/

**No software or hardware execution occurred in this research redirect.**
