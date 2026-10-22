"""
services/colab-relay-service.py
OOPS MVC Service Wrapper -> Canonical Root colab_relay_server.py
Preserves Standalone Colab zero-dependency execution while providing
OOPS MVC service hierarchy reference.
"""
import os
import sys

# Add repository root to python search path
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if BASE_DIR not in sys.path:
    sys.path.insert(0, BASE_DIR)

ROOT_RELAY = os.path.join(BASE_DIR, "colab_relay_server.py")

if __name__ == "__main__":
    if os.path.exists(ROOT_RELAY):
        with open(ROOT_RELAY, "r", encoding="utf-8") as f:
            code = compile(f.read(), ROOT_RELAY, "exec")
            exec(code, {"__name__": "__main__", "__file__": ROOT_RELAY})
    else:
        print(f"[ERROR] Standalone relay script not found at {ROOT_RELAY}")
        sys.exit(1)
