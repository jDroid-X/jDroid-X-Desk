"""
jDroid-X-Desk Colab Relay Service (services/colab-relay-service.py)
OOPS MVC Encapsulated Service Layer
Strangler Proxy & Forwarder to canonical colab_relay_server.py
"""

import os
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CANONICAL_SCRIPT = os.path.join(BASE_DIR, "colab_relay_server.py")

def main():
    if not os.path.exists(CANONICAL_SCRIPT):
        print(f"[ERROR] Canonical Colab Relay script not found at {CANONICAL_SCRIPT}")
        sys.exit(1)
    
    # Execute canonical script with forwarded environment and arguments
    with open(CANONICAL_SCRIPT, "r", encoding="utf-8") as f:
        code = compile(f.read(), CANONICAL_SCRIPT, "exec")
        exec(code, {"__name__": "__main__", "__file__": CANONICAL_SCRIPT})

if __name__ == "__main__":
    main()
