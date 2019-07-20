using Science;
using System;

namespace Pandaros.API.Research
{
    public class ResearchCompleteEventArgs : EventArgs
    {
        public ResearchCompleteEventArgs(PandaResearchable research, ColonyScienceState player)
        {
            Research = research;
            Manager = player;
        }

        public PandaResearchable Research { get; }

        public ColonyScienceState Manager { get; }
    }
}
