# Using SQUAD

See (https://github.com/bradygaster/squad/)[https://github.com/bradygaster/squad/]

## 1. Install Squad

```bash
npm install -g @bradygaster/squad-cli
squad init
```
## 2. Authenticate with GitHub (for Issues, PRs, and Ralph)

```bash
gh auth login
```

**✓ Validate:** Run `gh auth status` — you should see "Logged in to github.com".

## 3. Open Copilot with the Agent

```
copilot --agent squad --yolo
```

  > **Why `--yolo`?** Squad makes many tool calls in a typical session. Without it, Copilot will prompt you to approve each one.

**✓ Validate:** Squad responds with team member proposals. Type `yes` to confirm — they're ready to work.

Squad proposes a team — each member named from a persistent thematic cast. You say **yes**. They're ready.

## 4. Get to work!

In the chat, use a prompt like this:

```
I'm starting a new project. Set up the team based on the [xxx] universe.
Here's what I'm building -- see the docs/prd-elevator-dispatch.md file for the product requirements.
```

---