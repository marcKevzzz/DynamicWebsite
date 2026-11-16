# Verification

Run `dotnet build` and `dotnet publish -c Release`. Start the app with `dotnet run` and open the local URL.

Use `tests/smoke.mjs` with a Codex browser tab, or verify these behaviors in the browser:

- On a fresh load, the cover reveals a centered image stack. The cards move outward as the headline appears, then the header and calls to action become available.
- Scroll a short distance to check that the pill navigation expands and shows the ruler, then return to the top. Hover the two main calls to action and move the pointer across the hero to check the image motion.
- Scroll through About, the eight Work cards, Services, and the team. Check that the Work count follows the cards and that the staggered gallery fits touch screens and reduced motion.
- Open the command menu with Ctrl/Cmd+K. Check filtering, arrow selection, Enter activation, and Escape focus restoration. Switch the theme and reload to confirm it persists.
- Open a Work card. Verify its details and destination link, then close with Escape and check that focus returns to the card.
- Check the home page at 375, 768, 1200, and 1600px widths for clipping or horizontal scrolling. Repeat with JavaScript disabled to check static navigation and content.
