using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace RMC
{
    public class Dialog_Recruit : Window
    {
        Pawn negotiator;
        static Vector2 buttonSize = new Vector2(160f, 40f);
        Vector2 scrollPosition;
        ArmyDef army;
        RankDef[] ranks;
        string[] editBuffers;
        int[] counts;

        int ticksInADay = 60000;
        float headerRectHeight = 58f;
        float rowHeight = 30f;
        float rightMargin = 16f;
        float rowWidth = 0f;
        float buttonAreaHeight = 100f;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(720f, 600f);
            }
        }

        public Dialog_Recruit(Pawn negotiator, bool radioMode)
        {
            this.negotiator = negotiator;
            this.army = ArmyDef.GetFactionArmy(negotiator.Faction);

            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.soundAppear = SoundDefOf.CommsWindow_Open;
            this.soundClose = SoundDefOf.CommsWindow_Close;

            SetCounts();
            ranks = new RankDef[army.rankList.Count];

            for (int i = 0; i < army.rankList.Count; i++)
                ranks[i] = army.rankList[i];
        }

        public override void DoWindowContents(Rect window)
        {
            float y = 0f;
            rowWidth = window.width - rightMargin;

            GUI.BeginGroup(window);

            window = window.AtZero();
            Rect headerRect = new Rect(0f, 0f, rowWidth, headerRectHeight);

            GUI.BeginGroup(headerRect);

            Rect factionRect = new Rect(0f, 0f, rowWidth, headerRectHeight / 2f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(factionRect, Faction.OfPlayer.Name.Truncate(factionRect.width, null));

            y += headerRectHeight / 2f;
            Rect negotiatiorRect = new Rect(0f, y, rowWidth, headerRectHeight / 2f);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(negotiatiorRect, negotiator.LabelShort);

            GUI.EndGroup();

            y += headerRectHeight / 2f;
            Widgets.ThingIcon(new Rect(0f, y, 30f, rowHeight), new Thing() { def = ThingDefOf.Silver }, 1f);
            Rect silverRect = new Rect(35f, y, rowWidth / 2f, rowHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(silverRect, negotiator.Map.resourceCounter.Silver.ToString());

            Rect costRect = new Rect(rowWidth / 2f, y, rowWidth / 2f, rowHeight);
            GUI.color = Color.red;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(costRect, "-" + GetUnitCost());

            y += rowHeight - 10f;
            Rect timeRect = new Rect(0f, y, rowWidth / 2f, rowHeight);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(timeRect, "Arrival time");

            Rect spawnTimeRect = new Rect(rowWidth / 2f, y, rowWidth / 2f, rowHeight);
            GUI.color = Color.red;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(spawnTimeRect, "~" + GetUnitSpawnTime() / ticksInADay + " day(s)");

            y += rowHeight;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.white;
            Widgets.DrawLineHorizontal(0f, y, rowWidth);

            y += 5f; //spacer
            Rect mainRect = new Rect(0f, y, window.width, window.height - (y + buttonAreaHeight));
            FillMainRect(mainRect);

            y += mainRect.height;
            Rect acceptButton = new Rect(window.width / 2f - buttonSize.x / 2f, y + ((buttonAreaHeight - buttonSize.y) / 2f), buttonSize.x, buttonSize.y);
            if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate(), true, false, true))
                AcceptAction();

            Rect resetButton = new Rect(acceptButton.x - 10f - buttonSize.x, acceptButton.y, buttonSize.x, buttonSize.y);
            if (Widgets.ButtonText(resetButton, "ResetButton".Translate(), true, false, true))
                ResetAction();

            Rect cancelButton = new Rect(acceptButton.xMax + 10f, acceptButton.y, buttonSize.x, buttonSize.y);
            if (Widgets.ButtonText(cancelButton, "CancelButton".Translate(), true, false, true))
                CancelAction();

            GUI.EndGroup();
        }

        float FillMainRect(Rect mainRect)
        {
            Text.Font = GameFont.Small;
            float y = 6f;
            float height = y + army.rankList.Count * rowHeight;
            Rect viewRect = new Rect(0f, 0f, rowWidth - 5f, height);
            Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
            float minY = this.scrollPosition.y - rowHeight;
            float maxY = this.scrollPosition.y + mainRect.height;

            for (int i = 0; i < ranks.Length; i++)
            {
                if (y > minY && y < maxY)
                {
                    Rect row = new Rect(0f, y, viewRect.width, rowHeight);
                    DrawTradeableRow(row, i);
                }

                y += rowHeight;
            }

            Widgets.EndScrollView();

            return y;
        }

        void DrawTradeableRow(Rect row, int index)
        {
            Text.Font = GameFont.Small;
            if (index % 2 == 1) Widgets.DrawLightHighlight(row);
            if (Mouse.IsOver(row)) Widgets.DrawHighlight(row);

            GUI.BeginGroup(row);

            Rect rankRect = new Rect(0f, 0f, row.width / 2f, row.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rankRect, ranks[index].label);

            float widgetWidth = 30f;
            float spacer = 5f;

            if(Widgets.ButtonText(new Rect(row.width - (widgetWidth * 3 + spacer * 2), 0f, widgetWidth, row.height), "-") && counts[index] > 0)
                editBuffers[index] = (--counts[index]).ToString();

            Rect countField = new Rect(row.width - (widgetWidth * 2 + spacer), 0f, widgetWidth, row.height);
            Widgets.TextFieldNumeric<int>(countField, ref counts[index], ref editBuffers[index], 0.0f, 99.0f);

            if (Widgets.ButtonText(new Rect(row.width - widgetWidth, 0f, widgetWidth, row.height), "+") && counts[index] < 99)
                editBuffers[index] = (++counts[index]).ToString();

            GenUI.ResetLabelAlign();

            GUI.EndGroup();
        }

        void AcceptAction()
        {
            Action action = delegate
            {
                MapUtilities.DestroyThingsInMap(negotiator.Map, ThingDef.Named("Silver"), (int)GetUnitCost());

                IncidentParms_Deploy incidentParms = new IncidentParms_Deploy();
                incidentParms.target = negotiator.Map;
                incidentParms.faction = negotiator.Faction;
                incidentParms.forced = true;
                incidentParms.reinforcements = UnitDef.CreateUnitFromArrays(ranks, counts);

                Find.Storyteller.incidentQueue.Add(DefDatabase<IncidentDef>.GetNamed("RMC_IncidentDef_Deploy"), Find.TickManager.TicksGame + GetUnitSpawnTime(), incidentParms, 240000);

                this.Close(true);
            };

            if (negotiator.Map.resourceCounter.Silver >= GetUnitCost())
            {
                action();
            }
            else
            {
                SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
                Messages.Message("MessageColonyCannotAfford".Translate(), MessageTypeDefOf.RejectInput, false);
            }

            Event.current.Use();
        }

        void ResetAction()
        {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);

            SetCounts();
        }

        void CancelAction()
        {
            this.Close(true);
            Event.current.Use();
        }

        float GetUnitCost()
        {
            return UnitDef.CreateUnitFromArrays(ranks, counts).GetUnitCost();
        }

        int GetUnitSpawnTime()
        {
            return UnitDef.CreateUnitFromArrays(ranks, counts).GetSpawnTime();
        }

        void SetCounts()
        {
            editBuffers = new string[army.rankList.Count];
            counts = new int[army.rankList.Count];
        }
    }
}
