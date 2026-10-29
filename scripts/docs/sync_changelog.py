#!/usr/bin/env python3
"""Copies RunPad's CHANGELOG.md into the site as docs/site/changelog.md.

CHANGELOG.md in the code repository is the only place release notes are written; this page is a
generated copy. Links relative to the code repository (ROADMAP.md, src/...) are pointed at its
main branch on GitHub, since they do not exist in this site.

Usage: sync_changelog.py [PATH_TO_RUN_PAD]   (default: ../invoklet, a clone next to this one)
Run it after every change to CHANGELOG.md and commit the result.
"""
import re
import sys
from pathlib import Path

REPO_URL = "https://github.com/tervforge/invoklet/blob/main/"
FRONT_MATTER = (
    "---\n"
    "title: Changelog\n"
    "description: What changed in each RunPad release.\n"
    "---\n\n"
    "<!-- Generated from CHANGELOG.md in tervforge/invoklet by scripts/docs/sync_changelog.py; "
    "edit it there. -->\n\n"
)
# ](target) where target is not a URL with a scheme, a #fragment or a root-relative path.
RELATIVE_LINK = re.compile(r"\]\((?![a-z][a-z0-9+.-]*:|#|/)([^)\s]+)\)", re.IGNORECASE)


def main() -> int:
    run_pad = Path(sys.argv[1] if len(sys.argv) > 1 else Path(__file__).resolve().parents[3] / "invoklet")
    source = run_pad / "CHANGELOG.md"
    if not source.is_file():
        print(f"{source} not found; pass the path to a invoklet clone.", file=sys.stderr)
        return 1
    text = source.read_text(encoding="utf-8").replace("\r\n", "\n")
    # Keep the site's package-manager description neutral when copying older release notes.
    text = text.replace("**NuGet manager** in the style of LINQPad's,", "**NuGet manager**")
    text = RELATIVE_LINK.sub(lambda m: f"]({REPO_URL}{m.group(1)})", text)
    target = Path(__file__).resolve().parents[2] / "docs" / "site" / "changelog.md"
    with target.open("w", encoding="utf-8", newline="\n") as output:
        output.write(FRONT_MATTER + text)
    print(f"Wrote {target}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
