# Contributing

Thank you for considering a contribution to N.I.N.A.'s documentation!

# Table of Contents
[TOC]

# Prerequisites
The documentation is using MkDocs. A in-depth guide on how to set it up and how to use MkDocs can be found on their project homepage ![MkDocs](https://www.mkdocs.org)
In summary you need:
- ![Python](https://www.python.org/)
- Pip `pip install --upgrade pip`
- MkDocs `pip install mkdocs`
- MkDocs Material `pip install mkdocs-material`
- A markdown editor of your choice

# Building the docs
MkDocs offers a neat built-in server feature to build and preview the documentation on your local machine on the fly. Each time a file is saved, the local server is updated automatically.
To run the server simply run the serve command and open your browser on the indicated address.
```mkdocs serve``` 

# Branching Model
This project is utilizing a standard git flow where it has the following branches  
* master: all officially released code  
* hotfix/<hotfixname>: used to fix issues inside master  
* release/<version>: when preparing a release with new features a temporary release branch is created for that new release  
* bugfix/<bugfixname>: issues that are found during a release will be fixed here  
* develop: a general develop branch that will contain unreleased new features  
* feature/<featurename>: new features that will be developed and merged to the develop branch  

[A more in-depth guide about this model can be found here](https://nvie.com/posts/a-successful-git-branching-model/)

The most relevant branches are master, release and develop. These branches all have a separate documentation on the homepage. 
This enables users that will use for example the released version of N.I.N.A. to have a separate documentation, compared the ones that use the nightly builds and already want to see new features described.

# Pull Requests
* For contributing to this documentation you should fork the repository
* Inside your fork you can make your changes
* Once you are finished with your planned changes it is time to put up a pull request from your fork to the master repository
* Make sure that only relevant changes are inside the pull request  
* Check that the documentation builds properly using the serve command
* Try to create **one pull request per feature**
* Create your pull requests for new features only against the **develop** branch  

# Updating the homepage
The documentation on the homepage is updated automatically via bitbucket pipelines. Each time a pull request is complete, the pipeline will build the docs and upload the changes to the homepage.
There is no action required by a contributor for this.
