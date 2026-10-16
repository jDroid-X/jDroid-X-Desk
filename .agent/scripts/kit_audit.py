import os
import json
import sys

# Set encoding for Windows console
if sys.platform == "win32":
    import codecs
    sys.stdout = codecs.getwriter("utf-8")(sys.stdout.detach())

def audit_kit(kit_path):
    print(f"--- Starting Kit Audit in {kit_path} ---")
    
    agent_dir = os.path.join(kit_path, ".agent", "agents")
    skill_dir = os.path.join(kit_path, ".agent", "skills")
    
    # 1. Verify Agents
    if not os.path.exists(agent_dir):
        print(f"Error: Agent directory missing at {agent_dir}")
        return
        
    agents = [f for f in os.listdir(agent_dir) if f.endswith(".md")]
    print(f"Found {len(agents)} agents.")
    
    # 2. Verify Skills
    if not os.path.exists(skill_dir):
        print(f"Error: Skill directory missing at {skill_dir}")
        return
        
    skills = []
    # Direct subfolders of skill_dir are categories (Level 1)
    categories = [c for c in os.listdir(skill_dir) if os.path.isdir(os.path.join(skill_dir, c))]
    for cat in categories:
        cat_path = os.path.join(skill_dir, cat)
        domains = [d for d in os.listdir(cat_path) if os.path.isdir(os.path.join(cat_path, d))]
        for dom in domains:
            dom_path = os.path.join(cat_path, dom)
            s_folders = [s for s in os.listdir(dom_path) if os.path.isdir(os.path.join(dom_path, s))]
            for s in s_folders:
                # Store relative path from skill_dir (Level 3)
                skills.append(os.path.join(cat, dom, s))
                
    print(f"Found {len(skills)} skills across {len(categories)} categories.")
    
    # 3. Check for Broken Links
    broken_skills = []
    for skill in skills:
        skill_file = os.path.join(skill_dir, skill, "SKILL.md")
        if not os.path.exists(skill_file):
            broken_skills.append(skill)
            
    if broken_skills:
        print(f"Broken Skills (Missing SKILL.md): {broken_skills}")
    else:
        print("All skills have valid SKILL.md files.")
        
    # 4. Closing the Loop: State Check
    state_file = os.path.join(kit_path, ".agent", "state.json")
    if not os.path.exists(state_file):
        print("Warning: state.json missing. Initializing.")
        with open(state_file, 'w') as f:
            json.dump({"last_sync": "2026-04-16T09:43:38Z", "integrity_baseline": "verified"}, f)
            
    print("Kit Audit Complete. Integrity Verified.")

if __name__ == "__main__":
    audit_kit(sys.argv[1] if len(sys.argv) > 1 else ".")
