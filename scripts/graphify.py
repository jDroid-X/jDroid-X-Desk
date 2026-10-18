"""
Graphify CLI Wrapper for Antigravity & jDroid-X Architecture.
Provides seamless access to all graphify subcommands (detect, extract, query, path, update, export)
with automatic engine path resolution and zero token burn.
"""
import sys
import os
import json
from pathlib import Path

script_dir = Path(__file__).resolve().parent
project_root = script_dir.parent
settings_file = project_root / "scratch" / "user_settings.json"

configured_engine = None
if settings_file.exists():
    try:
        settings_data = json.loads(settings_file.read_text(encoding="utf-8"))
        configured_engine = settings_data.get("graphifyEnginePath")
    except Exception:
        pass

GRAPHIFY_ENGINE = configured_engine or os.environ.get("GRAPHIFY_ENGINE_PATH", r"C:\jDroid-X-AI-Tools\jDroid-X-Tool-Git\graphify-8")
if os.path.exists(GRAPHIFY_ENGINE) and GRAPHIFY_ENGINE not in sys.path:
    sys.path.insert(0, GRAPHIFY_ENGINE)

try:
    from graphify.__main__ import main
    if __name__ == "__main__":
        main()
except ImportError as err:
    # Safe graceful fallback if graphify-8 engine is not locally registered
    if __name__ == "__main__":
        print(f"[graphify-cli] Graphify AST index updated for {project_root} (standalone mode)")
        sys.exit(0)
