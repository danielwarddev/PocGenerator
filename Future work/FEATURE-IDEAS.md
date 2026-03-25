# Feature Ideas — PocGenerator (Overnight MVP Factory)

A brainstorm of features that could take PocGenerator from a solid MVP generator to an even more powerful autonomous development tool.

---

## 1. Parallel File Generation

Generate multiple independent files concurrently instead of one at a time. Use a bounded `SemaphoreSlim` (e.g., 3–5 concurrent requests) to speed up the generation phase without blowing past rate limits. Files with no dependency on each other (models, DTOs, isolated services) can be generated in parallel, while files that reference others are queued after their dependencies land on disk.

**Impact:** Could cut generation time by 50–70% for larger projects.

---

## 2. Interactive Progress Dashboard (TUI)

Replace plain console logging with a rich terminal UI using [Spectre.Console](https://spectreconsole.net/) or [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui). Show a live dashboard with:

- Current phase and step (Planning → Generating file 4/12 → Verifying…)
- Request budget gauge (e.g., `██████░░░░ 23/50 requests`)
- Real-time streaming of Copilot responses
- Elapsed time and ETA
- Color-coded build/test results

**Impact:** Much better developer experience when watching a generation run.

---

## 3. Multi-Language / Multi-Framework Support

Extend beyond .NET to support other tech stacks:

- **Node.js / TypeScript** (Express, Next.js, Astro)
- **Python** (FastAPI, Django, Flask)
- **Go** (Chi, Gin)
- **Rust** (Axum, Actix)

Each stack would have its own scaffolder, build/test commands, and validation strategy. The system prompt and planning phase would be parameterized by target stack.

**Impact:** Opens the tool up to a much wider range of SaaS ideas and developer preferences.

---

## 4. Attempt History & Comparison Dashboard

Build a local web UI (or CLI report) that lets you browse past generation attempts:

- Side-by-side diff of two attempts for the same idea
- Metrics per attempt: request count, time, build errors encountered, final pass/fail
- Filterable by idea, date, success/failure
- "Retry this idea" button that re-runs with the same or tweaked parameters

Store attempt metadata in a local SQLite database or structured JSON log.

**Impact:** Makes it easy to track improvement over time and identify patterns in failures.

---

## 5. Idea Scoring & Auto-Prioritization

Automate idea selection beyond the planned SPEC-09 Idea Picker. Feed all candidate ideas to Copilot and ask it to score them on:

- Technical feasibility for an MVP
- Fakeable data surface area (can it be convincingly demoed with mock data?)
- Estimated complexity (number of files, UI pages, API endpoints)
- Market interest signals (if idea notes include research)

Rank ideas and auto-pick the best one, or present a ranked list for the user to confirm.

**Impact:** Removes the manual step of choosing which idea to build next.

---

## 6. Post-Generation Smoke Test Runner

After the build and unit tests pass, automatically launch the generated app and run a basic smoke test:

- For web apps: start the server, use Playwright (already wired as an MCP server) to navigate key pages, check for HTTP 200s, take screenshots
- For console apps: run with sample arguments, verify exit code 0 and expected output patterns
- For APIs: send a few cURL/HTTP requests to key endpoints

Save screenshots and response logs as artifacts alongside the generated MVP.

**Impact:** Catches runtime failures that compile + unit tests miss — a much stronger "it actually works" signal.

---

## 7. Prompt Tuning Feedback Loop

After each generation run, record what worked and what didn't:

- Which files needed the most fix iterations?
- What categories of build errors recurred?
- Which system prompt phrases correlated with better first-pass success?

Use this data to automatically adjust system prompts over time — e.g., if Entity Framework migrations are a recurring pain point, add explicit guidance to the prompt. Could be as simple as appending "lessons learned" bullet points to the system prompt from a growing knowledge base file.

**Impact:** The tool gets smarter with every run, reducing wasted requests on recurring mistakes.

---

## 8. GitHub Repository Auto-Creation

After a successful generation, automatically:

1. `git init` the output directory
2. Create a `.gitignore` tailored to the project type
3. Make an initial commit
4. Create a new GitHub repo via the `gh` CLI
5. Push the MVP
6. Optionally create a GitHub release with a summary

**Impact:** Zero-friction path from "idea" to "published repo" — the MVP is immediately shareable.

---

## 9. Docker & Docker Compose Generation

Generate a `Dockerfile` and `docker-compose.yml` alongside the MVP so it can be run in a container out of the box. For web apps with a database layer, the compose file would include the app service plus a local database container. Verify the container builds and starts as part of the validation phase.

**Impact:** Makes generated MVPs instantly deployable and portable.

---

## 10. Cost & Token Usage Tracking

Track and display detailed usage metrics per run:

- Number of premium requests used (already have the hard cap counter)
- Estimated input/output token counts per request
- Cost estimate based on model pricing (configurable rates)
- Cumulative cost across all attempts
- Per-phase breakdown (planning used X requests, generation used Y, fixes used Z)

Output a summary at the end of each run and append to a running cost ledger.

**Impact:** Full visibility into what each generation costs, helping tune prompts and caps for efficiency.

---

## 11. Resumable Runs

If a run fails mid-way (crash, network issue, manual cancellation), persist a checkpoint file with:

- Current phase and step
- Files already generated
- Remaining plan items
- Session state / conversation history summary

On next run, detect the checkpoint and offer to resume from where it left off instead of starting over.

**Impact:** Saves budget and time when long runs are interrupted — no need to re-generate files that already passed validation.

---

## 12. Architecture Style Selector

Let the user (or auto-detect from the idea) choose an architecture style:

- **Vertical Slice** — feature folders, each with handler + model + test
- **Clean Architecture** — layers (Domain, Application, Infrastructure, Presentation)
- **Minimal / Flat** — everything in one project, great for tiny MVPs
- **Modular Monolith** — multiple bounded contexts in separate projects

Each style would influence the scaffolding scripts, the file plan, and the system prompt guidance.

**Impact:** Generates code that follows a coherent, recognizable architecture rather than an ad-hoc structure.

---

## 13. Generated CI/CD Pipeline

Include a `.github/workflows/ci.yml` (or equivalent) in the generated MVP that:

- Builds the solution
- Runs tests
- Optionally deploys to Azure App Service, Railway, or Fly.io (with placeholder secrets)

The pipeline file would be tailored to the project type and framework.

**Impact:** The generated MVP is CI-ready from minute one.

---

## 14. Idea-to-Landing-Page Mode

Add an alternate mode that, instead of generating a full MVP, generates a simple marketing landing page for the idea:

- Hero section with value proposition
- Feature highlights
- Fake testimonials / social proof
- Email signup form (mocked)
- Responsive design with Tailwind CSS or similar

Great for quickly validating whether an idea "looks right" before investing in the full MVP.

**Impact:** Fast, cheap way to gut-check an idea's presentation before committing to a full build.

---

## 15. Plugin / Extension System

Allow users to drop custom plugins into a `plugins/` folder that hook into the pipeline:

- **Pre-scaffold plugins** — add custom NuGet packages, npm packages, or config files
- **Post-generate plugins** — run linters, formatters, or custom validation scripts
- **Post-finalize plugins** — deploy, notify (Slack/Discord webhook), archive

Plugins could be simple PowerShell/bash scripts or .NET assemblies implementing a plugin interface.

**Impact:** Makes PocGenerator extensible without forking — users can customize the pipeline for their workflow.

---

## 16. Multi-Idea Batch Mode

Accept a list of ideas (from a file or directory of `*.md` files) and generate MVPs for all of them sequentially or in parallel:

```powershell
dotnet run --project PocGenerator -- --batch ./ideas/
```

Each idea gets its own output directory, attempt log, and success/failure status. At the end, print a summary table showing which ideas succeeded and which failed.

**Impact:** "Set it and forget it" overnight — wake up to multiple MVPs instead of just one.

---

## 17. Quality Gate Configuration

Let users define custom quality gates beyond "builds and tests pass":

- Minimum test count threshold (e.g., at least 10 tests)
- Code coverage minimum (via Coverlet)
- No `TODO` or `HACK` comments in generated code
- Maximum cyclomatic complexity per method
- Required files must exist (e.g., `README.md`, `appsettings.json`)

Quality gate failures would trigger additional fix loop iterations.

**Impact:** Ensures generated MVPs meet a minimum quality bar before being considered "done."

---

## 18. Semantic Diff Review

After generation completes, produce a human-readable summary of what was built:

- List of all generated files with one-line descriptions
- Key design decisions Copilot made (e.g., "chose SQLite over in-memory because…")
- Dependency graph visualization (which projects reference which)
- API endpoint summary (for web projects)
- Entity/model diagram (Mermaid)

Output as a `GENERATION-REPORT.md` in the MVP directory.

**Impact:** Makes it much faster to understand and review what the AI built, especially for larger MVPs.
