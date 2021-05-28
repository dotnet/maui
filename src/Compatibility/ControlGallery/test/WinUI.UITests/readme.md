# Running the UI Tests on UWP

To run the tests you'll need to install the latest release of [WinAppDriver](https://github.com/Microsoft/WinAppDriver). 

Make sure you've built and deployed the UWP version of ControlGallery on the machine where you're running the tests. Run WinAppDriver.exe (if you installed it in the default location, it will be "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe").

With WinAppDriver running, start the tests with whatever test runner you normally use. The tests will take care of launching ControlGallery. 
 
Now just sit back and watch the tests run. At the moment, WinAppDriver and our implementation of IApp for UWP have to take control of the mouse and keyboard to run these tests, so interacting with the test machine will probably interfere with the tests. (You may be able to work around this by running the tests in the Simulator; I've had mixed results and some blue screens when trying this, but good luck.)

Currently we can only run the tests against the desktop UWP application.

