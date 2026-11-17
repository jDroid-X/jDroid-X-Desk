"""
jDroid-X-Desk YAML Connection Runner (run_yaml_connection.py)
Strangler Forwarder (Chesterton's Fence) -> services/yaml-connection-service.py
"""

import os
import sys

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
SERVICE_SCRIPT = os.path.join(BASE_DIR, "services", "yaml-connection-service.py")

if __name__ == "__main__":
    if os.path.exists(SERVICE_SCRIPT):
        with open(SERVICE_SCRIPT, "r", encoding="utf-8") as f:
            code = compile(f.read(), SERVICE_SCRIPT, "exec")
            exec(code, {"__name__": "__main__", "__file__": SERVICE_SCRIPT})
    else:
        print(f"[ERROR] Service script not found at {SERVICE_SCRIPT}")
        sys.exit(1)
