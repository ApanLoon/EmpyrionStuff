#!/bin/sh

## Full path to Config_Example.ecf:
config="/cygdrive/g/Steam/steamapps/common/Empyrion - Galactic Survival/Content/Configuration/Config_Example.ecf"

cat "$config" | sed 's/\r//' | gawk '\
BEGIN\
{
    id="";
    name="";
    ref="";
    category="";
    inBlock=0;
} \
/{ Block Id:/\
{
    match($0,/^. Block Id: ([0-9]+), Name: ([^,]*)(, Ref: (.*))?/,capture);
    id=capture[1];
    name=capture[2];
    ref=capture[4];
    inBlock=1;
} \
/Category:/\
{
    match($0,/^. Category: (.*)/,capture);
    category=capture[1];
}
/}/\
{
    if (inBlock==1)
    {
         printBlock(id, name, ref, category);
    }
    inBlock=0;
}\
END\
{
    if (inBlock==1)
    {
         printBlock(id, name, ref, category);
    }
}\
function printBlock(id, name, ref, category)
{
    print category "|" id "|" name "|" ref
}\
' | sort -t\| -k1 | gawk -F\| '\
BEGIN\
{
    print "        public static readonly Dictionary<UInt16, EpbBlockType> BlockTypes = new Dictionary<UInt16, EpbBlockType>()";
    print "        {"
    category="";
} \
/.*/\
{
    if ($1 != category)
    {
        print "\n            // " $1;
    }
    category=$1;
    id=$2;
    name="\"" $3 "\"";
    cat="\"" $1 "\""; 
    ref="\"" $4 "\"";
    printf "            {%5d, new EpbBlockType(){Id = %5d, Name = %-31s, Category = %-31s, Ref = %-31s}},\n", id, id, name, cat, ref;
} \
END\
{
    print "        };";
}\
'

