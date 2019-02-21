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
    print "        public enum EpbBlockType";
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
    printf "            %-31s = %5d, // %s\n", $3, $2, $4;
} \
END\
{
    print "        }";
}\
'

