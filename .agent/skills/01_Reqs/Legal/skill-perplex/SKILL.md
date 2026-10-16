---
name: skill-perplex
description: "Skill for parallel reading relevant useful information using the Perplexity API to enhance the active project."
risk: low
source: custom
date_added: "2026-04-21"
---

## Use this skill when

- You need to perform parallel research using the Perplexity API.
- You need to enhance the active project with real-time information, best practices, or updated documentation.
- The user requests deep-dive research using the `mcp_perplexity-ask_perplexity_ask` tool.

## Do not use this skill when

- The information is already available in the local project context.
- You need to perform local file system operations without any need for external knowledge.

## Instructions

- Clearly define the research objective and identify the key knowledge gaps in the current project.
- Formulate precise questions for the Perplexity API.
- If multiple distinct topics need research, formulate parallel queries.
- Parse the responses from the Perplexity API and synthesize them into a cohesive report or direct code implementation plan.
- Ensure all information gathered is contextually relevant to the active project's architecture and constraints.

## Focus Areas

- Leveraging the `mcp_perplexity-ask_perplexity_ask` tool efficiently.
- Parallelizing requests when dealing with multifaceted research topics.
- Synthesizing external knowledge into project-specific recommendations.

## Approach

1. Analyze the project needs and formulate a list of specific questions.
2. Call the `mcp_perplexity-ask_perplexity_ask` tool with the formulated queries.
3. Review the responses, extracting actionable insights.
4. Integrate the findings into the project documentation, or use them to guide code generation.

## Output

- A structured summary of the gathered information.
- Actionable recommendations tailored to the active project.
- Code snippets or architectural designs based on the latest best practices retrieved.

## Limitations
- Ensure API usage is optimized to avoid rate limits or quota issues.
- Always cross-reference the generated advice with the project's specific constraints.
