#!/bin/bash -eux

# find if there are any duplicated simulator runtimes for a given platform

set -o pipefail
IFS=$'\n\t'

xcrun simctl runtime list -j > simruntime.json
cat simruntime.json

grep -e '"identifier" : ' -e '"runtimeIdentifier" : ' simruntime.json | tr '\n' ' ' | sed -e 's/,//g' -e 's/"//g' -e 's/runtimeIdentifier : //g' -e $'s/identifier : /@/g' | tr '@' '\n' | awk NF | sed 's/^[[:blank:]]*//' > simruntime-lines.txt
cat simruntime-lines.txt

sed -e 's/.*com.apple/com.apple/g' simruntime-lines.txt > simruntime-runtimes.txt
cat simruntime-runtimes.txt

sort simruntime-runtimes.txt | uniq -c | sort -n | sed 's/^[[:blank:]]*//' > simruntime-runtimes-by-count.txt
cat simruntime-runtimes-by-count.txt

grep -v '^1 ' simruntime-runtimes-by-count.txt | sed 's/^[0-9 ]*//' > simruntime-duplicated-runtimes.txt
cat simruntime-duplicated-runtimes.txt

while IFS= read -r simruntime
do
  echo "Duplicated: $simruntime"
  grep "$simruntime" simruntime-lines.txt | sed 's/ .*//' | while IFS= read -r id
  do
    echo "    sudo xcrun simctl runtime delete $id"
    if ! sudo xcrun simctl runtime delete "$id"; then
      echo "    failed to delete runtime $id"
    else
      echo "    deleted runtime $id"
    fi
  done
done < simruntime-duplicated-runtimes.txt

xcrun simctl runtime list -v