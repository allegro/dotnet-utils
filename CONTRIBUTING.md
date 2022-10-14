# Contributing

## Pull requests

* for small changes, no need to add separate issue, defining problem in pull request is enough
* if issue exists, reference it from PR title or description using GitHub magic words like *resolves #issue-number*
* before making significant changes, please contact us via issues to discuss your plans and decrease number of changes after Pull Request is created
* create pull requests to **main** branch
* when updating a package, make sure to:
  * update its README.md
  * update its CHANGELOG.md
  * update its version
  * consider presenting usage in Demo app (`/samples`)
  * add tests
* when creating a new package, make sure to:
  * maintain the repo structure (see existing packages)
  * add code analyzers
  * create the package's README.md 
  * initialize the package's CHANGELOG.md
  * add tests
  * consider presenting usage in Demo app (`/samples`)
  * reference the package from README.md in repo's root

## Coding style

The coding style is guarded by the analyzers (such as stylecop) and .editorconfig. 
Make sure to follow the defined standards. 