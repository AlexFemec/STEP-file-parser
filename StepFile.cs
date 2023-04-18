using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;



namespace STEP_Parser
{
    public class StepFile
    {
        public StepFile(string fn, EXPRESS_Schema schema)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            this.filename = fn;
            this.AP203 = schema;

            EntityParse(fn);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Step File Parsed in " + elapsedMs.ToString() + " ms");
        }

        public string filename;
        public Dictionary<int, Entity> Entitys;
        public EXPRESS_Schema AP203;

        private Dictionary<int, List<int>> parentData = new Dictionary<int, List<int>>();



        public void EntityParse(string fn)
        {
            TextReader tr = new StreamReader(fn);
            string stepstring;
            string line;
            string datastring;
            string[] stringarray;
            stepstring = tr.ReadToEnd();

            while ((line = tr.ReadLine()) != null)
            {
                stepstring += line;
            }

            int i1 = stepstring.IndexOf("DATA;");
            datastring = stepstring.Substring(i1 + 7);
            int i2 = datastring.IndexOf("ENDSEC;");
            datastring = datastring.Substring(0, i2 - 1);
            datastring = datastring.Replace("\r", "");
            datastring = datastring.Replace("\n", "");
            stringarray = datastring.Split(';');

            string[] str;
            string[] identifier = new string[stringarray.Length];
            int[] id_array = new int[stringarray.Length];
            Match m1;
            Match m2;
            Match m_complex;
            Regex g = new Regex(@"(?<=(\= ))[^\(]*(?=(\())");

            
            //Regex gData = new Regex(@"(?<=\()[^\(].*");

            //Regex gData = new Regex(@"(\((?>[^()]+|(?1))*\))");
            Regex gData = new Regex(@"\(((?>[^()]+|\((?<n>)|\)(?<-n>))+(?(n)(?!)))\)");
            


            Regex complexmatch = new Regex(@"[\=]\s*[\(].*[\) ;]");

            string[] datastr = new string[stringarray.Length];

            List<Entity> entityList = new List<Entity>();
            Entitys = new Dictionary<int, Entity>();

            string str_name;
            string str_dat;

            for (int i = 0; i < stringarray.Length; i++)
            {
                if ((!stringarray[i].Contains("#")) || (!stringarray[i].Contains("="))) continue;

                try
                {
                    str = stringarray[i].Split('=');
                    str[0] = str[0].Replace("#", "");
                    int id = Convert.ToInt32(str[0]);
                    id_array[i] = id;
                    m1 = g.Match(stringarray[i]);
                    m_complex = complexmatch.Match(stringarray[i]);

                    if (m1.Success & m1.Length>0 & m_complex.Length<1)
                    {
                        str_name = m1.Groups[0].Value.Trim(); //name
                        m2 = gData.Match(stringarray[i]);
                        if (m1.Success)
                        {
                            //str_dat = m2.Groups[0].Value.Trim().TrimEnd(')');
                            str_dat = m2.Groups[1].Value.Trim();
                            Entity ent = new Entity(id, str_name, str_dat, AP203);
                            Entitys[id] = ent;
                        }
                    }

                    if (m_complex.Success && m_complex.Length > 0)
                    {
                        // int t = 0;
                        str_dat = m_complex.Groups[0].Value.Trim(); //data
                        str_dat = str_dat.TrimStart('=', '(');
                        str_dat = str_dat.TrimEnd(';');
                        str_dat = str_dat.TrimEnd(')');

                        List<int> splitLocations = new List<int>();
                        splitLocations.Add(0);
                        int level = 0;
                        for (int j = 0; j < str_dat.Length; j++)
                        {
                            if (str_dat[j] == '(') level++;
                            if (str_dat[j] == ')') level--;
                            if (str_dat[j] == ')' && level == 0)
                            {
                                splitLocations.Add(j + 1);
                            }
                        }
                        List<string> entList = new List<string>();
                        for (int j = 1; j < splitLocations.Count; j++)
                        {
                            entList.Add(str_dat.Substring(splitLocations[j - 1], splitLocations[j] - splitLocations[j - 1]).Trim());
                        }

                        List<string> types = new List<string>();

                        for (int j = 0; j < entList.Count; j++)
                        {
                            int location1 = entList[j].IndexOf(' ');
                            int location2 = entList[j].IndexOf('(');
                            int location;

                            if (location1 > 0) location = Math.Min(location1, location2);
                            else location = location2;

                            string type = entList[j].Substring(0, location);
                            types.Add(type);
                        }
                        EntityComplex EntC = new EntityComplex(id, types, entList, this.AP203);
                        Entitys.Add(id, EntC);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception while parsing entity " + ex.ToString());
                }
            }



            foreach (KeyValuePair<int, Entity> entry in Entitys)
            {
                List<dynamic> flattenedAttributeList = GetFlattenedAttributes(entry.Value.attributesConverted);

                foreach (dynamic item in flattenedAttributeList)
                {
                    if (item.GetType() == typeof(RefID))
                    {
                        List<int> existingList;

                        if (parentData.TryGetValue(item.id, out existingList)) //if key exists add to the existing parent list
                        {
                            parentData[item.id].Add(entry.Value.entityID);
                        }
                        else  //otherwise create a key and new list
                        {
                            parentData[item.id] = new List<int> { entry.Value.entityID };
                        }
                    }
                }
            }
        }


        //get flattened list of all attributes including those in sub-lists
        public List<dynamic> GetFlattenedAttributes(List<dynamic> inputList)
        {
            if (inputList == null) return new List<dynamic>();

            List<dynamic> flattenedList = new List<dynamic>();
            foreach (dynamic listItem in inputList)
            {
                if (listItem is System.Collections.IList)
                {
                    flattenedList.AddRange(GetFlattenedAttributes(listItem));
                }
                else
                {
                    flattenedList.Add(listItem);
                }
            }
            return flattenedList;
        }


        public List<int> GetAllParents(int id)
        {
            try
            {
                var test = parentData[id];
                return parentData[id];
            }
            catch
            {
                return new List<int>();
            }
        }


        public List<int> GetChildren(int id)
        {
            if (id==-1)
            {
                return new List<int>();
            }

            Entity e1 = Entitys[id];
            List<int> childIDs = new List<int>();

            if (e1.attributesConverted==null) return new List<int>();

            foreach (dynamic item in e1.attributesConverted)
            {
                if (item.GetType() == typeof(RefID))
                {
                    childIDs.Add(item.id);
                }
                if (item.GetType() == typeof(List<object>))
                {
                    foreach (dynamic element in item)
                    {
                        if (element.GetType() == typeof(RefID))
                        {
                            childIDs.Add(element.id);
                        }
                    }
                }
            }
            return childIDs;
        }


        public List<int> GetAllChildren(int id)
        {
            Entity e1 = Entitys[id];
            List<int> childIDs = new List<int>();

            foreach (dynamic item in e1.attributesConverted)
            {
                if (item.GetType() == typeof(RefID))
                {
                    childIDs.Add(item.id);
                    childIDs.AddRange(GetAllChildren(item.id));
                }
                if (item.GetType() == typeof(List<object>))
                {
                    foreach (dynamic element in item)
                    {
                        if (element.GetType() == typeof(RefID))
                        {
                            childIDs.Add(element.id);
                            childIDs.AddRange(GetAllChildren(element.id));
                        }
                    }
                }
            }
            return childIDs;
        }


        public string GetTypeOfEnt(int id)
        {
            return Entitys[id].type;
        }


        //search by type and subtypes
        public List<int> searchTreeByType(int id, string type)
        {
            List<string> typesToSearchFor = AP203.GetAllChildren(type);
            typesToSearchFor.Add(type.ToLower());
            List<int> children = GetAllChildren(id);
            List<int> found = new List<int>();
            foreach (int cid in children)
            {
                foreach (string subtype in typesToSearchFor)
                {
                    if (Entitys[cid].type.ToLower() == subtype) found.Add(cid);
                }
            }
            return found;
        }


        public List<int> GetFirstParent(int id)
        {
            List<int> parents = GetAllParents(id);

            if (parents.Count == 0)
            {
                return new List<int> { id };
            }
            else
            {
                List<int> firstParents = new List<int>();

                foreach (int p in parents)
                {
                    firstParents.AddRange(GetFirstParent(p));

                }
                return firstParents;
            }
        }


        public dynamic GetAttributeValueByName(int id, string attributeName)
        {
            if (id < 1) return null;

            attributeName = attributeName.ToLower();
            List<AttributeDefinition> ADList = new List<AttributeDefinition>();
            ADList = AP203.GetEntityByName(Entitys[id].type).attributeDefinitionsFullTree;

            int foundID = -1;
            for (int i = 0; i < ADList.Count; i++)
            {
                if (ADList[i].name == attributeName) foundID = i;
            }

            Entity test = Entitys[id];

            if (foundID > -1)
            {
                return Entitys[id].attributesConverted[foundID];
            }
            else
            {
                return null;
            }
        }


        public dynamic GetAttributeValueByNameComplex(EntityComplex cpe, string attributeName)
        {
            for (int i = 0; i < cpe.attributeDefinitions.Count; i++)
            {
                if (cpe.attributeDefinitions[i].name == attributeName)
                {
                    return cpe.attributesConverted[i];
                }
            }
            return null;
        }


        public List<int> GetTopLevelEntities()
        {
            List<int> topLevelIDs = new List<int>();

            HashSet<int> topLevel = new HashSet<int>();
            HashSet<int> hasParents = new HashSet<int>();

            foreach (KeyValuePair<int, Entity> e in Entitys)
            {
                topLevel.Add(e.Key);
            }

            foreach (KeyValuePair<int, List<int>> entry in parentData)
            {
                hasParents.Add(entry.Key);
            }

            topLevel.ExceptWith(hasParents);
            topLevelIDs = topLevel.ToList();

            return topLevelIDs;
        }


    }

}


