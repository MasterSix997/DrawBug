﻿// Credits To ZimGUI (MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.LowLevel;

namespace Drawbug
{
    internal enum InsertType {
        Before,
        After,
        First,
        Last,
    }
    internal static class PlayerLoopInserter {

        internal static void InsertSystem(Type thisLoop,Type parentLoopType,InsertType insertType,PlayerLoopSystem.UpdateFunction function) {
            var mySystem = new PlayerLoopSystem
            {
                type = thisLoop,
                updateDelegate = function,
            };
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            switch (insertType) {
                case InsertType.First :{ var subSystemList = playerLoop.subSystemList.AsSpan();
                    foreach (ref var subSystem in subSystemList) {
                        if (subSystem.type == parentLoopType) {
                            subSystem.subSystemList = subSystem.subSystemList.Prepend(mySystem).ToArray();
                            break;
                        }
                    }
                    break;
                }
                case InsertType.Last :{ var subSystemList = playerLoop.subSystemList.AsSpan();
                    foreach (ref var subSystem in subSystemList) {
                        if (subSystem.type == parentLoopType) {
                            subSystem.subSystemList = subSystem.subSystemList.Append(mySystem).ToArray();
                            break;
                        }
                    }
                    break;
                }
                case InsertType.Before :{ 
                    var subSystemList = RemoveRunner(playerLoop,thisLoop);
                    for (var index = 0; index < playerLoop.subSystemList.Length; index++) {
                        if (subSystemList[index].type == parentLoopType) {
                            playerLoop.subSystemList = playerLoop.subSystemList.Insert(index, mySystem).ToArray();
                            break;
                        }
                    }

                    break;
                } 
                case InsertType.After:{ 
                    var subSystemList = RemoveRunner(playerLoop,thisLoop);
                    for (var index = 0; index < playerLoop.subSystemList.Length; index++) {
                        if (subSystemList[index].type == parentLoopType) {
                            playerLoop.subSystemList = playerLoop.subSystemList.Insert(index+1, mySystem).ToArray();
                            break;
                        }
                    }

                    break;
                }
            }
           
            
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static PlayerLoopSystem[] RemoveRunner(PlayerLoopSystem loopSystem,  Type loopRunnerType)
        {
            
            return loopSystem.subSystemList
                .Where(ls =>   ls.type != loopRunnerType)
                .ToArray();
        }

        private static PlayerLoopSystem[] RemoveRunner(PlayerLoopSystem loopSystem,  Type loopRunnerType1,Type loopRunnerType2)
        {
            
            return loopSystem.subSystemList
                .Where(ls =>   ls.type != loopRunnerType1&&ls.type!=loopRunnerType2)
                .ToArray();
        }
        internal static void RemoveRunner( Type loopRunnerType)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            playerLoop.subSystemList = RemoveRunner(playerLoop, loopRunnerType);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
        internal static void RemoveRunner(  Type loopRunnerType1,Type loopRunnerType2)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            playerLoop.subSystemList = RemoveRunner(playerLoop, loopRunnerType1,loopRunnerType2);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        internal static IEnumerable<T> Insert<T>(this IEnumerable<T> enumerable,int index,T element) {
            var current = 0;
            foreach (var e in enumerable) {
                if (current++ == index) {
                    yield return element;
                }
                yield return e;
            }
        }
    }
}