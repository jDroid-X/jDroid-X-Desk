# jDroid-X Offline AI Sync Workflow

This guide details the closed-loop development and operational workflow designed for managing and running local AI models on your `jDroid-X` USB drive using Ollama and Cline.

## Workflow Visualization

![jDroid-X 7-Stage MVC Closed-Loop Workflow](file:///C:/Users/jiten/jAnitGravity/.kit/.agent/workflows/jdroid_x_workflow_diagram.png)

---

## The 7-Stage Closed-Loop Process

1. **Initiate & Plan:** Define feature improvements, such as adding terminal-based Cline launching capabilities.
2. **Requirements:** Establish local model parameters (`ollama launch cline --model <name>`).
3. **MVC Design:** 
   * **Model:** Sync registry via `model_aliases.json`.
   * **View:** User-facing markdown and text ledgers (`Antigravity Local Model.txt`).
   * **Controller:** Synchronizer python script (`sync_model_mcp_manifest.py`).
4. **Implementation:** Update files, rename "Portable" references to "jDroid-X", and update command templates.
5. **Testing:** Run the sync controller to verify text output generation.
6. **Deployment:** Distribute updated configurations to the USB drive and local cache.
7. **Maintenance & Support (Feedback):** Execute the commands directly from the terminal guide for day-to-day coding.
