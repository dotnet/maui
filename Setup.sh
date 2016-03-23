#!/bin/bash
secrets="./Xamarin.Forms.Controls/secrets.txt"
mapskey="./Xamarin.Forms.ControlGallery.Android/Properties/MapsKey.cs"

if [ -f "$secrets" ]
then
	echo "$secrets found."
else
	echo "$secrets not found."
	touch $secrets
	echo "$secrets created."
fi

if [ -f "$mapskey" ]
then
	echo "$mapskey found."
else
	echo "$mapskey not found."
	touch $mapskey
	echo "using System.Reflection;" >> $mapskey
	echo "using System.Runtime.CompilerServices;" >> $mapskey
	echo "using System.Runtime.InteropServices;" >> $mapskey
	echo "using Android.App;" >> $mapskey
	echo "[assembly: Android.App.MetaData(\"com.google.android.maps.v2.API_KEY\", Value = \"\")]" >> $mapskey
	echo "$mapskey created."
fi