namespace Gherkin3.Ast
{
    public class Scenario : ScenarioDefinition
    {
        public Scenario(Tag[] tags, Location location, string keyword, string name, string description, Step[] steps) 
            : base(tags, location, keyword, name, description, steps)
        {
        }
    }
}