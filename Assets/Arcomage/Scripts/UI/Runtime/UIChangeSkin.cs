using System.Collections.Generic;
using UnityEngine;
// using Spine;
// using GamePlay;

namespace NOAH.UI
{
    public class UIChangeSkin : MonoBehaviour
    {
        private List<List<string>> skinGroup=new List<List<string>>();
        private List<int> skinIndex=new List<int>();

        // private ActorVisualSpine actorVisualSpine;
        private int SkinGroupCount=3;

        //ts调用
        public void SetData(string name)
        {
            // actorVisualSpine = GameObject.Find("Player:"+name+";"+name)?.GetComponentInChildren<ActorVisualSpine>();
            
        }

        public void ChangeSkin(int index,int add)
        {
            int tmpindex = skinIndex[index]+ add;
            if (tmpindex < 0) tmpindex = skinGroup[index].Count - 1;
            else tmpindex %= skinGroup[index].Count;
            skinIndex[index] = tmpindex;

            List<string> tmpSkin = new List<string>();
            for(int i = 0; i < SkinGroupCount; ++i)
            {
                string skin = skinGroup[i][skinIndex[i]];
                if (skin != "None") tmpSkin.Add(skin);
            }

            // actorVisualSpine.UISetMixSkin(tmpSkin);
        }

        public string GetCurentSkinName(int index)
        {
            return skinGroup[index][skinIndex[index]];
        }
    }
}
