appfolder="$1"
rootfolder="$2"
changesfile="$3"
platformkeyword="$4"

previousrunfolder="$rootfolder/PreviousUITestResults"
should_run_tests=false

echo "Looking for $previousrunfolder"

if test -e $previousrunfolder
then
	echo "Found previous build's test results folder"
else
	should_run_tests=true
	echo "Previous test results folder not found"
fi

echo "Looking for $previousrunfolder/nunit_output.xml"

if test -e $previousrunfolder/nunit_output.xml
then
	echo "Found previous NUnit results"
else
	should_run_tests=true
	echo "Previous NUnit results not found"
fi

if test -e $changesfile
then
	echo "Found list of changes:"
	changes_require_tests=false
	cat $changesfile
	
	# Look for platform-specific code changes
	if grep -q $platformkeyword $changesfile
	then
		echo "Found changes to $platformkeyword stuff"
		should_run_tests=true
		changes_require_tests=true
	fi

	# Now check for universal stuff (Core, Issues...)
	if grep -q -e 'Xamarin\.Forms\.Controls' -e 'Xamarin\.Forms\.Core' -e 'Xamarin\.Forms\.CustomAttributes' -e 'Xamarin\.Forms\.Xaml' $changesfile
	then
		echo "Found changes which require a UI Tests run"
		should_run_tests=true
		changes_require_tests=true
	fi
	
	if ! $changes_require_tests
	then
		echo "No changes which require a UI Tests run for $platformkeyword were found"
	fi
	
else
	should_run_tests=false
	echo "No changes in this build"
fi

if $should_run_tests
then
	echo "Can't reuse previous results; UI tests will be run."
	touch $appfolder/noresults.xml
	exit 1
else
	echo "We can just reuse the previous test results"
	echo "##teamcity[importData type='nunit' path='$previousrunfolder/nunit_output.xml' parseOutOfDate='true']"
	cp $previousrunfolder/nunit_output.xml $appfolder/nunit_output.xml
	exit 0
fi