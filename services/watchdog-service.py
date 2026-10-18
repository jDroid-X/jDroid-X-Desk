"""
jDroid-X-Desk Watchdog Keepalive Service (services/watchdog-service.py)
OOPS MVC Encapsulated Service Layer
Strangler Proxy & Forwarder to canonical colab-watchdog-keepalive.py
"""

import os
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CANONICAL_SCRIPT = os.path.join(BASE_DIR, "colab-watchdog-keepalive.py")

def main():
    if not os.path.exists(CANONICAL_SCRIPT):
        print(f"[ERROR] Canonical Watchdog script not found at {CANONICAL_SCRIPT}")
        sys.exit(1)
    
    with open(CANONICAL_SCRIPT, "r", encoding="utf-8") as f:
        code = compile(f.read(), CANONICAL_SCRIPT, "exec")
        exec(code, {"__name__": "__main__", "__file__": CANONICAL_SCRIPT})

if __name__ == "__main__":
    main()
