using System.Text.RegularExpressions;
using UnityEngine;

namespace ECS.Scripts.Testing
{
    public class TestingRegEx : MonoBehaviour
    {
        [SerializeField] private string inputText = "test@gmail.com";
        [SerializeField] private string regexPattern = "(.+)@(.+)\\.(\\w+)";

        private void OnValidate()
        {
            //Creates a new regex object from the pattern
            Regex regex = new Regex(regexPattern);
            
            //Finds matches in the input text
            Match regexMatch = regex.Match(inputText);

            if (regexMatch.Success)
            {
                //Successful match - output message
                print($"Matched text: {regexMatch.Groups[0]}");
                
                //Iterate through all groups in the match
                for (int i = 0; i < regexMatch.Groups.Count; i++)
                    print($"Group {i}: {regexMatch.Groups[i].Value}");
            }
        }
    }
}