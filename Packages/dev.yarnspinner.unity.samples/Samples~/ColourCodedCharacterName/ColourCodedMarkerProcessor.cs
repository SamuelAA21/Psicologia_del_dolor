using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

public class ColourCodedMarkerProcessor : ReplacementMarkupHandler
{
    private LineProviderBehaviour lineProvider;

    public override ReplacementMarkerResult ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes, string localeCode)
    {
        Debug.Log("Processing " + marker.Name);
        return new ReplacementMarkerResult();

    }

    public void Start()
    {
        if (lineProvider == null)
        {
            var runner = DialogueRunner.FindRunner(this);
            if (runner == null)
            {
                Debug.LogWarning("Was unable to find a dialogue runner, unable to register the style markup.");
                return;
            }
            lineProvider = (LineProviderBehaviour)runner.LineProvider;
        }
        lineProvider.RegisterMarkerProcessor("character", this);
    }
}
