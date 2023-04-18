using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace STEP_Parser
{
    public class EXPRESS_Schema 
    {
        public EXPRESS_Schema(string filename )
        {
            //initialize:
            entityDefinitionList = new Dictionary<string, EntityDefinition>();
            typeAliasesReal = new List<string>();
            typeAliasesInt = new List<string>();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            schemaFileName = filename;           

            ParseSchema();
            ParseType();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Schema Parsed in " + elapsedMs.ToString() + " ms");
        }

        public EXPRESS_Schema(string[] filenames)
        {
            //initialize:
            entityDefinitionList = new Dictionary<string, EntityDefinition>();
            typeAliasesReal = new List<string>();
            typeAliasesInt = new List<string>();


            var watch = System.Diagnostics.Stopwatch.StartNew();


            foreach (string filename in filenames)
            {
                schemaFileName = filename;
                ParseSchema();
                ParseType();
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Schema Parsed in " + elapsedMs.ToString() + " ms");
        }




        string schemaFileName;
        Dictionary<string, EntityDefinition> entityDefinitionList;


        List<string> typeAliasesReal = new List<string>();
        List<string> typeAliasesInt = new List<string>();

        public void ParseType()
        {
            TextReader tr = new StreamReader(schemaFileName);
            string stepstring;
            string line;
            string datastring;
            string[] stringarray;
            string[] str;
            stepstring = tr.ReadToEnd();

            while ((line = tr.ReadLine()) != null)
            {
                stepstring += line;
            }

            stringarray = stepstring.Split('\r');

            string value;
            string typename;
            int idx;

            for (int i = 0; i < stringarray.Length; i++)
            {
                stringarray[i] = stringarray[i].Trim();
                if (stringarray[i].StartsWith("TYPE") && stringarray[i].Contains('='))
                {
                    stringarray[i] = stringarray[i].Replace(';', ' ');
                    idx = stringarray[i].IndexOf("=");
                    typename=stringarray[i].Substring(4,idx-4).Trim();
                    value = stringarray[i].Substring(idx+1, stringarray[i].Length-idx-1 ).Trim();

                    if (value == "REAL") typeAliasesReal.Add(typename);
                    if (value == "INTEGER") typeAliasesInt.Add(typename); 
                }
            }

            for (int i = 0; i < stringarray.Length; i++)
            {
                stringarray[i] = stringarray[i].Trim();
                if (stringarray[i].StartsWith("TYPE") && stringarray[i].Contains('='))
                {
                    stringarray[i] = stringarray[i].Replace(';', ' ');
                    idx = stringarray[i].IndexOf("=");
                    typename = stringarray[i].Substring(4, idx - 4).Trim();
                    value = stringarray[i].Substring(idx + 1, stringarray[i].Length - idx - 1).Trim();

                    if (typeAliasesReal.Contains(value)) typeAliasesReal.Add(typename);
                    if (typeAliasesInt.Contains(value)) typeAliasesInt.Add(typename);
                }
            }

            for (int i = 0; i < typeAliasesReal.Count(); i++)
            {
                typeAliasesReal[i] = typeAliasesReal[i].ToUpper();
            }
            for (int i = 0; i < typeAliasesInt.Count(); i++)
            {
                typeAliasesInt[i] = typeAliasesInt[i].ToUpper();
            }
 
        }




        public void  ParseSchema()
        {
            TextReader tr = new StreamReader(schemaFileName);
            string infile_string;
            infile_string = tr.ReadToEnd();

            List<int> indexesStart = AllIndexesOf(infile_string, "ENTITY");
            List<int> indexesEnd = AllIndexesOf(infile_string, "END_ENTITY");

            List<string> entries = new List<string>();
            for (int i = 0;  i<indexesStart.Count(); i++ )
            {
                string substr = infile_string.Substring(indexesStart[i], indexesEnd[i] - indexesStart[i]);
                entries.Add(substr);
            }


            List<string> sections = new List<string>(new string[] { "SUBTYPE OF", "SUPERTYPE OF", "ABSTRACT SUPERTYPE", "WHERE", "DERIVE", "UNIQUE", ";" });


            for (int i = 0;  i<entries.Count(); i++ )
            {
                List<int> indexesSplit = AllIndexesOf(entries[i], sections);
                indexesSplit.Insert(0,0);
                indexesSplit.Add(entries[i].Length);
                List<string> subsections = new List<string>();
                for (int j = 1; j < indexesSplit.Count(); j++)
                {
                    string subsection = entries[i].Substring(indexesSplit[j - 1], indexesSplit[j] - indexesSplit[j - 1]).Replace(';', ' ').Trim();
                    if (subsection.Length > 0)  subsections.Add(subsection);
                }

                string name = subsections[0].Substring(7, subsections[0].Length - 7).Trim();
                string supertypes="";
                string subtypes="";

                int endOfAttributes;
                for (endOfAttributes = 1; endOfAttributes < subsections.Count(); endOfAttributes++)
                {
                    if (subsections[endOfAttributes].Contains("WHERE")) break;
                    if (subsections[endOfAttributes].Contains("DERIVE")) break;
                    if (subsections[endOfAttributes].Contains("UNIQUE")) break;
                    if (subsections[endOfAttributes].Contains("INVERSE")) break;
                    
                }
                endOfAttributes--;


                List<AttributeDefinition> attributeList = new List<AttributeDefinition>();
                List<string> supertypesList = new List<string>();
                List<string> subtypesList = new List<string>();
               
                for (int j = 1; j <= endOfAttributes; j++)
                {

                    if (subsections[j].Contains("SUBTYPE OF"))
                    {
                        supertypes = subsections[j].Substring(11,subsections[j].Length-11) ;
                    }
                    else if (subsections[j].Contains("SUPERTYPE OF"))
                    {
                        subtypes = subsections[j].Substring(12, subsections[j].Length - 12);
                    }
                    else if (subsections[j].Contains("ABSTRACT SUPERTYPE"))
                    {
                        //TODO
                    }
                    else //assume its an atribute
                    {
                        int indexof = subsections[j].IndexOf(':');
                        if (indexof >1)
                        {
                            string str1 = subsections[j].Substring(0, indexof).Trim();
                            string str2 = subsections[j].Substring(indexof+1, subsections[j].Length-indexof-1).Trim();

                            if (str1.StartsWith("OPTIONAL"))
                            {
                                str1 = str1.Replace("OPTIONAL", "").Trim();
                            }
                            
                            AttributeDefinition at = new AttributeDefinition(str1, str2);
                            attributeList.Add(at);
                        }
                    }

                    subtypesList = new List<string>();
                    supertypesList = new List<string>();

                    if (subtypes.Length > 1)
                    {
                        subtypes = subtypes.Replace("\r\n", "");
                        subtypes = subtypes.Replace("ONEOF", "");
                        subtypes = subtypes.Replace("ANDOR", "");
                        subtypes = subtypes.Replace("AND", "");
                        subtypes = subtypes.Replace("OR", "");

                        List<string> parts = subtypes.Split(new Char[] { ',', '(', ')' }).ToList();
                        for (int k = parts.Count()-1; k >=0 ; k--)
                        {
                            parts[k] = parts[k].Trim();
                            if (parts[k].Length < 1) parts.RemoveAt(k);
                        }

                        subtypesList=parts;
                    }

                    if (supertypes.Length > 1)
                    {
                        supertypes = supertypes.Replace("\r\n", "");
                        supertypes = supertypes.Replace("ONEOF", "");
                        supertypes = supertypes.Replace("ANDOR", "");
                        supertypes = supertypes.Replace("AND", "");
                        supertypes = supertypes.Replace("OR", "");

                        List<string> parts = supertypes.Split(new Char[] { ',', '(', ')' }).ToList();
                        for (int k = parts.Count() - 1; k >= 0; k--)
                        {
                            parts[k] = parts[k].Trim();
                            if (parts[k].Length < 1) parts.RemoveAt(k);
                        }

                        supertypesList=parts;
                    }
                }

                EntityDefinition e1 = new EntityDefinition();
                e1.name = name;
                e1.attributeDefinitions = attributeList;
                e1.subtypes = subtypesList; 
                e1.supertypes = supertypesList;

                entityDefinitionList.Add(name, e1);
            }

            foreach (var item in entityDefinitionList)
            {
                EntityDefinition definition = item.Value;
                List<string> parents = definition.supertypes;
                if (parents.Count == 0) //nothing to inherit
                {
                    definition.attributeDefinitionsFullTree = definition.attributeDefinitions;
                }
                else  //traverse tree
                {
                    List<string> allParents = GetAllParents(definition.name);
                    definition.attributeDefinitionsFullTree = new List<AttributeDefinition>();
                    for (int j = allParents.Count - 1; j >= 0; j--)
                    {
                        EntityDefinition e1 = GetEntityByName(allParents[j]);
                        if (e1 != null)
                        {
                            definition.attributeDefinitionsFullTree.AddRange(e1.attributeDefinitions); //parents' attibutes
                        }
                    }
                    definition.attributeDefinitionsFullTree.AddRange(definition.attributeDefinitions); //this entity's attibutes
                }
            }

        }


        public static List<dynamic> StringToListRec(string data)
        {
            data = data.Trim();
            string buffer = "";
            int bufferLength = 0;
            int startidx = 0;
            List<dynamic> DL = new List<dynamic>();
            int index = 0;
            bool escaping=false;

            //check for string with one open bracket and one close bracket only
            int count1 = data.Split('(').Length - 1;
            int count2 = data.Split(')').Length - 1;
            int count3 = data.Split(',').Length - 1;
            if (count1 == 1 && count2 == 1 && count3 == 0)
            {
                buffer = "";
                int start = data.IndexOf('(');
                int end = data.IndexOf(')');
                if (start < end)
                {
                    buffer = data.Substring(start+1, end - start -1).Trim();
                }
                DL.Add(buffer);
                return DL;
            }
            
            while (index < data.Length)
            {
                if (data[index] == '\'')
                {
                    if (escaping == false) escaping = true;
                    else
                    {
                        escaping = false;
                    }
                }

                switch (data[index])
                {
                    case '(':
                        if (escaping) break;
                        int level = 0;
                        startidx = index + 1;
                        while ((data[index] != ')' || level != 0) && index < data.Length - 1)
                        {
                            if (data[index] == '(') level++;
                            if (data[index + 1] == ')') level--;
                            index++;
                        }

                        buffer = data.Substring(startidx, index - startidx);
                        startidx = index + 1;

                        if ((!buffer.Contains('(')) && (!buffer.Contains(',')))
                        {
                            
                            DL.Add(buffer.Trim());
                        }
                        else
                        {
                            DL.Add(StringToListRec(buffer));
                        }
     
                        buffer = "";
                        bufferLength = 0;
                        break;
                    case ')':
                        if (escaping) break;
                        startidx = index + 1;
                        bufferLength = 0;
                        break;
                    case ',':
                        if (escaping) break;
                        if (bufferLength > 0)
                        {
                            buffer = data.Substring(startidx, bufferLength);
                            DL.Add(buffer);
                        }
                        startidx = index + 1;
                        buffer = "";
                        bufferLength = 0;
                        break;
                    default:
                        bufferLength++;
                        break;
                }

              if (index == data.Length - 1 && bufferLength > 0)
                    //if (bufferLength > 1)
                    {
                    int testlen = data.Length;
                    if (startidx + bufferLength > data.Length) bufferLength = data.Length - startidx - 1;
                    if (bufferLength > 0)
                    {
                        buffer = data.Substring(startidx, bufferLength);
                        DL.Add(buffer);
                    }
                }
                index++;
            }
            return DL;
        }



        public List<string> GetAllParents(string name)
        {
            var entity = GetEntityByName(name);
            if (entity == null) return new List<string>();

            List<string> superTypeList = entity.supertypes;
            List<string> allParents  = new List<string>(superTypeList);

            for (int i = 0; i < superTypeList.Count(); i++)
            {
                List<string> superTypeList2 = GetAllParents(superTypeList[i]);
                allParents.AddRange(superTypeList2);
            }

            //remove duplicates
            List<string> unique = allParents.Distinct().ToList();
            return unique;
        }


        public List<string> GetAllChildren(string name)
        {
            name = name.ToLower();
            List<string> subTypeList = new List<string>();
            foreach (var item in entityDefinitionList)
            {
                EntityDefinition ed = item.Value;
                if (ed.supertypes.Contains(name))
                {
                    subTypeList.Add(ed.name);
                }
            }
            subTypeList.AddRange(GetEntityByName(name).subtypes);
            subTypeList = subTypeList.Distinct().ToList();

            List<string> allChildren = new List<string>(subTypeList);

            for (int i = 0; i < subTypeList.Count(); i++)
            {
                List<string> subTypeList2 = GetAllChildren(subTypeList[i]);
                allChildren.AddRange(subTypeList2);

            }
            //remove duplicates
            List<string> unique = allChildren.Distinct().ToList();
            return unique;
        }



        
      
        public EntityDefinition GetEntityByName(string name)
        {
            name = name.ToLower();
            try
            {
                return entityDefinitionList[name];
            }
            catch
            {
                return null;
            }
        }

        
       
        public List<int> AllIndexesOf(string haystack, string needle)
        {
            int index1, index2;

            if (String.IsNullOrEmpty(needle))
                throw new ArgumentException("the string to find is empty", "needle");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += needle.Length)
            {
                index1 = haystack.IndexOf(" " + needle, index);
                index2 = haystack.IndexOf("\n" + needle, index);
                if (index1 != -1) index = index1;
                else if (index2 != -1) index = index2;
                else index = -1;
                if (index == -1)
                    break;
                indexes.Add(index);
            }
            return indexes;
        }

        public List<int> AllIndexesOf(string haystack, List<string> needles)
        {
            List<int> indexes = new List<int>();
            string needle = "";
            for (int j = 0; j < needles.Count(); j++)
            {
                needle = needles[j];
                for (int index = 0; ; index += needle.Length)
                {
                    index = haystack.IndexOf(needle, index);
                    if (index == -1)
                        break;

                    indexes.Add(index);
                }
            }

            indexes.Sort();
            return indexes;
        }




        public dynamic StringToMappedType(dynamic attributeValue, string attributeType, EXPRESS_Schema AP203 )
        {
            attributeType=attributeType.ToUpper();
            if (attributeValue.GetType() == typeof(string))
            {
                string s = (string)attributeValue.Trim();
                if (s.StartsWith("#"))
                {
                    s = s.Substring(1, s.Length - 1);
                    int result;
                    try
                    {
                        result = Convert.ToInt32(s);
                        return new RefID(result);
                    }
                    catch
                    {
                        return new RefID(-1);
                    }
                }
                else if (attributeType == "LABEL")
                {
                    s = s.Replace("\"", "");
                    s = s.Replace("\'", "");
                    return s;
                }
                else if (AP203.typeAliasesReal.Contains(attributeType) || (attributeType == "REAL"))    
                {
                    double result;
                    try
                    {
                        result = Convert.ToDouble(attributeValue);
                    }
                    catch
                    {
                        return "parse error";
                    }
                    return result;
                }
                else if (AP203.typeAliasesInt.Contains(attributeType) || (attributeType == "INTEGER"))
                {
                    int result;
                    try
                    {
                        result = Convert.ToInt32(attributeValue);

                    }
                    catch
                    {
                        return "parse error";
                    }

                    return result;
                }
                else if ((attributeType == "BOOLEAN") || (attributeType == "LOGICAL"))
                {
                    if (s.Contains('T')) return true;
                    if (s.Contains('F')) return false;
                    return null;
                }
                else
                {
                    return attributeValue;
                }
            }

            if (attributeValue.GetType() == typeof(List<object>))
            {
                if (attributeType.StartsWith("LIST") || attributeType.StartsWith("SET") )
                {
                    List<dynamic> newList = new List<dynamic>();
                    int idx = attributeType.IndexOf("OF");
                    string type = attributeType.Substring(idx + 2, attributeType.Length - idx - 2).Trim().ToLower();
                    for (int i = 0; i < attributeValue.Count; i++)
                    {
                        newList.Add(StringToMappedType(attributeValue[i], type, AP203));
                    }
                    return newList;
                }
            }
            return "";
        }
    }


    struct RefID
    {
        public RefID(int id)
        {
            this.id = id;
        }
        public int id;
    }

    public class AttributeDefinition
    {
        public string name;
        public string type;
        public int id; //?
        public AttributeDefinition(string name, string dataType)
        {
            this.name = name;
            this.type = dataType;
        }
        public AttributeDefinition()
        {
        }
    }


    public class Attribute
    {
        public Attribute(AttributeDefinition attributeDefinition, dynamic value)
        {
            this.value = value;
            this.name = attributeDefinition.name;
            this.type = attributeDefinition.type;
        }
        public string name;
        public string type;
        dynamic value;
    }


    public class EntityDefinition
    {
        public string name;
        public List<AttributeDefinition> attributeDefinitions;
        public List<AttributeDefinition> attributeDefinitionsFullTree;
        public List<string> subtypes;
        public List<string> supertypes;
    }


    public class Entity
    {
        public int entityID;
        public string type;
        public List<dynamic> attributesConverted;
        public int status;
        public bool isComplex;

        public Entity()
        {
          
        }

        public Entity(int id, string type, string attributeString, EXPRESS_Schema AP203)
        {
            this.entityID = id;
            this.type = type;
            this.isComplex = false;

            if (id==131)
            {

            }

            EntityDefinition ed = AP203.GetEntityByName(type);

            if (ed==null)
            {
                Console.WriteLine("entity definition not found in schema: id=" + id + "   type: " + type);
                this.status = -1;
                return;
            }

            //TEST
            EXPRESS_Schema.StringToListRec("'1','2','3'");
            EXPRESS_Schema.StringToListRec("'part',$");

            List<dynamic> attributes = EXPRESS_Schema.StringToListRec(attributeString);
            
            if  (attributes.Count != ed.attributeDefinitionsFullTree.Count)
            {
                Console.WriteLine("parsing error on entity #" + id);
                this.status=-1;
                return;
            }
            attributesConverted = new List<dynamic>();
            for (int i = 0; i < attributes.Count; i++)
            {
                dynamic a = AP203.StringToMappedType(attributes[i], ed.attributeDefinitionsFullTree[i].type,AP203);
                attributesConverted.Add(a);
            }
        }

    }



    public class EntityComplex:Entity
    {
        public List<string> types;
        public List<dynamic> attributeValues;
        public List<AttributeDefinition> attributeDefinitions;
       
        public EntityComplex(int id, List<string> types, List<string> attributeStrings, EXPRESS_Schema AP203)
        {
            this.entityID = id;
            this.types = types;
            this.isComplex = true;
            this.type = "COMPLEX";
            attributeValues = new List<dynamic>();
            attributeDefinitions = new List<AttributeDefinition>();
            attributesConverted = new List<dynamic>();

            if (id==76)
            {

            }


            for (int i = 0; i < types.Count; i++)
            {
                EntityDefinition ed = AP203.GetEntityByName(types[i]);
                attributeDefinitions.AddRange(ed.attributeDefinitions);
                List<dynamic> values = EXPRESS_Schema.StringToListRec(attributeStrings[i]);
                if (values[0].GetType() == typeof(List<object>))
                {
                    attributeValues.AddRange(values[0]);
                }
                else if (values[0].GetType() == typeof(string))
                    if (values[0].Length > 0)
                    {
                        attributeValues.Add(values[0]);
                    }
            }

            if (attributeValues.Count != attributeDefinitions.Count)
            {
                //TODO
            }


            for (int i = 0; i < attributeDefinitions.Count; i++)
            {
                dynamic a = AP203.StringToMappedType(attributeValues[i], attributeDefinitions[i].type, AP203);
                attributesConverted.Add(a);
            }

        }
    }

}
