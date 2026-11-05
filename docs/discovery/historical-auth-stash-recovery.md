# Historical recovery: API management authentication stash

The local stash `049cf672629f4db2e83bdef3558f6928ee74648a` is retained as a
parent of this recovery commit, including its index and third parents. It
contains an older Codex rules migration based on authentication commit
`6e4dca81eae62fadc8aaa108935c38ef684f82f1`, which is not an ancestor of the
current main branch.

The historical authentication code and rules are not applied to the current
tree. The old configuration disables issuer, audience and HTTPS metadata
validation; it is not approved for deployment. This archive is not a security
review or evidence that the old behavior works with current main.

Inspect the changes with `git diff <stash>^1 <stash>` and inspect the other
parents with `git show <stash>^2` and `git ls-tree -r <stash>^3`. Review any
recovered content before use. The original stash remains local.

A squash merge would omit the historical parents. Preserve the original stash
and its parents separately before squashing or removing the source branch.
