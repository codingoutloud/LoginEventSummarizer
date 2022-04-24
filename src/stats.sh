#!/bin/bash

echo -n "Country count: "
cat ipcc.csv | sed 's/.*,//' | sort | uniq | wc -l

echo -n "IP addr count: "
cat ipcc.csv | sed 's/,.*//' | sort | uniq | wc -l
