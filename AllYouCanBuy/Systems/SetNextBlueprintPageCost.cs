using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy.Systems
{
    [UpdateAfter(typeof(CreateRerollCostTracker))]
    [UpdateBefore(typeof(PreventDurationByCost))]
    [UpdateBefore(typeof(RerollShopAfterDuration))]
    [UpdateBefore(typeof(RerollBlueprintView.UpdateRerollBlueprintView))]
    public class SetNextBlueprintPageCost : GameSystemBase, IModSystem
    {
        internal static int ProgressViewIdentifier { get; private set; }

        private int _rerollCost;
        private bool _pagingEnabled;
        private EntityQuery _rerollTriggers;
        private float _rerollDuration;
        private bool _durationConfigured;

        protected override void Initialise()
        {
            base.Initialise();
            _rerollTriggers = GetEntityQuery(typeof(CRerollShopAfterDuration), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            UpdateProgressViewIdentifier();

            var isPagingDay = HasSingleton<SDay>() && GetSingleton<SDay>().Day % 5 != 0;

            if (isPagingDay && !_pagingEnabled && HasSingleton<SRerollCost>())
            {
                _rerollCost = GetSingleton<SRerollCost>().Cost;
                _pagingEnabled = true;
            }

            if (isPagingDay && _pagingEnabled)
            {
                SetSingleton(new SRerollCost { Cost = 0 });
                ConfigureDuration(1f);
            }
            else if (!isPagingDay && _pagingEnabled)
            {
                if (HasSingleton<SRerollCost>())
                {
                    SetSingleton(new SRerollCost { Cost = _rerollCost });
                }

                RestoreDuration();
                _pagingEnabled = false;
            }
        }

        private void UpdateProgressViewIdentifier()
        {
            using var entities = _rerollTriggers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!EntityManager.HasComponent<CHasIndicator>(entity))
                {
                    continue;
                }

                var indicator = EntityManager.GetComponentData<CHasIndicator>(entity);
                if (indicator.IndicatorType != ViewType.ProgressView ||
                    !EntityManager.HasComponent<CLinkedView>(indicator.Indicator))
                {
                    continue;
                }

                ProgressViewIdentifier = EntityManager
                    .GetComponentData<CLinkedView>(indicator.Indicator)
                    .Identifier.Identifier;
                return;
            }
        }

        private void ConfigureDuration(float total)
        {
            using var entities = _rerollTriggers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var duration = EntityManager.GetComponentData<CTakesDuration>(entity);
                if (!_durationConfigured)
                {
                    _rerollDuration = duration.Total;
                    _durationConfigured = true;
                }

                SetDuration(entity, duration, total);
            }
        }

        private void RestoreDuration()
        {
            if (!_durationConfigured)
            {
                return;
            }

            using var entities = _rerollTriggers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                SetDuration(entity, EntityManager.GetComponentData<CTakesDuration>(entity), _rerollDuration);
            }

            _durationConfigured = false;
        }

        private void SetDuration(Entity entity, CTakesDuration duration, float total)
        {
            if (Mathf.Approximately(duration.Total, total))
            {
                return;
            }

            var remaining = duration.Total > 0f
                ? Mathf.Clamp01(duration.Remaining / duration.Total) * total
                : total;
            duration.Total = total;
            duration.Remaining = remaining;
            EntityManager.SetComponentData(entity, duration);
        }
    }
}
