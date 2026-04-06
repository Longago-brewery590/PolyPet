# Impeccable Skill Compatibility for Unity

[impeccable.style](https://impeccable.style/) is a Claude Code skill pack designed for frontend/web design.
These notes cover which commands are useful for this Unity project and which are not.

## Start here: `/teach-impeccable`

Run this once before anything else. It scans the codebase for existing design docs (art style, theme,
elevation shadow, etc.), asks questions about users, brand personality, and aesthetic direction, then
writes a `## Design Context` section to `.impeccable.md` in the project root. All subsequent skills
read that file for project context — so `/colorize`, `/critique`, `/animate` etc. will be grounded in
Siemens branding and the LED floor context rather than generic web assumptions.

When prompted, also append the Design Context to `CLAUDE.md` so it persists across all sessions.

This skill has no web-specific assumptions and works fully for Unity projects.

## Works (platform-agnostic design thinking)

| Command | What it provides |
|---------|-----------------|
| `/critique` | UX evaluation framework: visual hierarchy, cognitive load checklist, Nielsen's 10 heuristics, emotional journey, information architecture. Universal concepts; examples are web-flavored but the reasoning transfers. |
| `/colorize` | Color strategy, palette design, semantic color (success/error/warning), 60/30/10 rule. Zero web-specific assumptions — applies directly to Unity materials and art direction. |
| `/distill` | Simplification philosophy: removing complexity, progressive disclosure, visual hierarchy. Applicable to game UI/UX decisions regardless of platform. |
| `/animate` | Motion design principles: timing curves, easing, purpose-driven animation, entrance/feedback/delight categories. Directly applicable to Unity Animator timelines and DOTween sequences. |
| `/clarify` | UX copy, error messages, microcopy, and label writing. Platform-agnostic — applies to any Unity UI text. |
| `/bolder` | Amplify safe or boring designs for more visual impact. Pure design thinking with no web assumptions. |
| `/quieter` | Tone down overly bold or visually aggressive designs. Inverse of `/bolder`, same reasoning applies. |

## Partial (principles transfer, code output won't)

These skills reason correctly about design but will emit CSS/JS implementation details.
Ignore the code — use the analysis and recommendations.

| Command | What transfers | What to ignore |
|---------|----------------|----------------|
| `/typeset` | Type hierarchy, scale ratios, readability, line length | `rem`, `clamp()`, CSS custom properties |
| `/arrange` | Spatial rhythm, grouping, density, visual hierarchy | CSS Grid, Flexbox, breakpoints |
| `/delight` | Delight strategy, surprise/discovery principles, contextual appropriateness | Framer Motion, confetti.js, CSS animations |
| `/optimize` | Rendering and animation performance principles | Bundle size, lazy loading, image formats, browser APIs |

## Web-only (not applicable)

| Command | Reason |
|---------|--------|
| `/audit` | WCAG, ARIA, semantic HTML, bundle size, touch targets |
| `/polish` | Browser compatibility, TypeScript, CSS design tokens, layout shift |
| `/harden` | i18n, RTL text, browser APIs, Intl formatting |
| `/overdrive` | WebGL shaders, WebGPU, View Transitions API, scroll-driven animations |
| `/frontend-design` | Builds web UI — the base skill all others invoke |
| `/normalize` | Normalizes to a CSS design system |
| `/extract` | Extracts reusable CSS/component patterns |
| `/adapt` | Responsive design across screen sizes and devices |
| `/onboard` | Web onboarding flows and first-time user experiences |
