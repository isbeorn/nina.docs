## Forking the repository

1. Login to your bitbucket account
2. Navigate to [N.I.N.A.'s main repository](https://bitbucket.org/Isbeorn/nina/) and click on the big plus
![Bitbucket Plus Sign](../images/setup/Fork1.png)
3. Click "Fork this repository"
![Fork this repository](../images/setup/Fork2.png)
4. You will see a configuration wizard for your fork.
5. Enter a name for your forked repository. (This guide will use "ninaexamplefork" for reference)
6. Give a short description what your intention is with this fork (optional)
7. Uncheck Issue tracking and Wiki as you don't need this in your fork.
8. Click on "Fork repository"
![Fork settings](../images/setup/Fork3.png)
9. You will be navigated to your new repository at https://bitbucket.org/\<your\_username\>/\<your\_fork's\_name\>/

## Cloning Repository

1. On the top right of your forked repository click on the "Clone" button
2. A pop up will show the command how to clone this repository to your local machine
3. Open a command window
4. Navigate to the folder where you want your repository folder to be created in
5. Enter the command that was shown in step 2.
```bash
git clone -n -b develop https://<YourUserName>@bitbucket.org/<YourUserName>/<YourForkName>.git
# NOTE: the -n flag for "don't checkout the branch"
# Ignore any LFS smudge errors for now. They are not yet synced and will get synced in a later step
```
6. Navigate to the created folder
```bash
cd <YourForkName>
```
7. Next you need to add the "upstream" to the root repository (where your fork is based on). This is later required for merging from the main dev branch etc.
```bash
git remote add upstream https://bitbucket.org/Isbeorn/nina.git
```
8. Bitbucket will not automatically copy over the LFS into the fork repository. This has to be done manually. Run the following commands to sync the lfs
```
git lfs fetch upstream --all
git lfs push origin --all
```
9. Now we need to checkout the develop branch
```
git checkout develop
```
10. Fetch the submodules for the external dependencies. These contain all third party vendor DLLs that are not part of any nuget package, e.g. Camera Drivers, VCRedist etc.  
```
git submodule update --init --recursive
```
## Navigation in Visual Studio

1. Navigate to your local clone and open "NINA.sln"
![Navigate to solution file](../images/setup/Clone4.png)
2. Visual studio has a built in git UI, which you can find on the lower right called "Team Explorer"
![Visual Studio Overview](../images/setup/VisualStudio1.png)
3. Inside the Team Explorer you can get a visual list of all available branches
![Team Explorer](../images/setup/VisualStudio2.png)
4. Here you can see all branches of your forked repository. Note that the upstream we added earlier is not yet displayed.4. Expand "remotes\origin" and double click on "develop" to switch to this branch
![Branches](../images/setup/VisualStudio3.png)
5. Click the back button (top left) and click on "Sync"
![Sync](../images/setup/VisualStudio4.png)
6. Click on "Fetch" and select "upstream" to fetch all branches that are inside "upstream", which is the root repository
![Fetch](../images/setup/VisualStudio5.png)
7. Now navigate back and select the "Branches" again. Now the "upstream" branches are also available.
![See branches and remotes](../images/setup/VisualStudio6.png)
8. To fetch and merge the latest code of the root repository you can right click on "develop", click on "fetch" and "Merge from..." afterwards.
9. Now your branch is up to date and you can start coding.
![Fetch code](../images/setup/VisualStudio7.png)
