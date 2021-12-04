#!/bin/bash
zeros=(0 0 0 0 0 0 0 0 0 0 0 0)
ones=(0 0 0 0 0 0 0 0 0 0 0 0)
lines=()

while : ; do
    read line
    [ -z "$line" ] && break
    lines+=($line)

    for i in ${!zeros[@]}; do
        if [ ${line:i:1} = "0" ]
        then
            zeros[i]=$((zeros[i]+1))
        else
            ones[i]=$((ones[i]+1))
        fi
    done
done


gamma=0
epsilon=0
for i in ${!zeros[@]}; do
  gamma=$((gamma*2))
  epsilon=$((epsilon*2))
  if [ ${zeros[i]} -gt ${ones[i]} ]
  then
    gamma=$((gamma+1))
  else
    epsilon=$((epsilon+1))
  fi
done

echo "${#lines[@]} lines"
echo "Gamma: $gamma"
echo "Epsilon: $epsilon"
echo "Step 1 - Power: $((gamma*epsilon))"


bitstring_to_int() {
  local result=0
  local source=$1
  for i in ${!zeros[@]}; do
      result=$((result*2))
      if [ ${source:i:1} = "1" ]
      then
          result=$((result+1))
      fi
  done
  converted_int=$result
}

calculate_rating() {
  local filteredLines=("${lines[@]}")

  local prioritizeValue=$1
  local takeMoreCommon=$2

  for i in ${!zeros[@]}; do
    local prioritizedLines=()
    local otherLines=()

    for l in ${!filteredLines[@]}; do
      local line=${filteredLines[l]}
      if [ ${line:i:1} = "$prioritizeValue" ]
      then
        prioritizedLines+=($line)
      else
        otherLines+=($line)
      fi
    done

    local mostMatches=()
    local leastMatches=()

    if [ ${#otherLines[@]} -eq ${#prioritizedLines[@]} ]
    then
      filteredLines=("${prioritizedLines[@]}")
    else
      if [ ${#otherLines[@]} -gt ${#prioritizedLines[@]} ]
      then
        mostMatches=("${otherLines[@]}")
        leastMatches=("${prioritizedLines[@]}")
      else
        mostMatches=("${prioritizedLines[@]}")
        leastMatches=("${otherLines[@]}")
      fi

      if [ ${takeMoreCommon} = "1" ]
      then
        filteredLines=("${mostMatches[@]}")
        [ ${#filteredLines[@]} -eq 0 ] && filteredLines=("${leastMatches[@]}")
      else
        filteredLines=("${leastMatches[@]}")
        [ ${#filteredLines[@]} -eq 0 ] && filteredLines=("${mostMatches[@]}")
      fi
    fi

  done
  bitstring_to_int ${filteredLines[0]}
  rating=$converted_int
}

calculate_rating "1" "1"
oxygen=$rating

calculate_rating "0" "0"
co2=$rating

echo "Oxygen: $oxygen"
echo "Co2: $co2"
echo "Step 2 - Rating: $((oxygen*co2))"
