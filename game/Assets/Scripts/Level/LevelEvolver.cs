using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror.BouncyCastle.Asn1.Esf;
using UnityEngine;
using Random = System.Random;

namespace Treep.Level {
    public static class IListExtensions {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> ts, Random rng) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = rng.Next(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
        }

        /// <summary>
        /// Returns element in a random order based on given RNG.
        /// </summary>
        public static IEnumerable<T> RandomOrderAccess<T>(this IList<T> list, Random rng) {
            int[] values = Enumerable.Range(0, list.Count).ToArray();
            values.Shuffle(rng);

            foreach (var index in values) {
                yield return list[index];
            }
        }
    }

    public class PlacedRoom {
        public RoomData Template { get; private set; }
        public Vector2 Position { get; private set; }

        public PlacedRoom(RoomData template, Vector2 position) {
            Template = template;
            Position = position;
        }

        public bool DoOverlap(PlacedRoom lastPlacedRoom) {
            var selfRect = Template.size;
            var otherRect = lastPlacedRoom.Template.size;

            return Position.x < lastPlacedRoom.Position.x + otherRect.x
                   && Position.x + selfRect.x > lastPlacedRoom.Position.x
                   && Position.y < lastPlacedRoom.Position.y + otherRect.y
                   && Position.y + selfRect.y > lastPlacedRoom.Position.y;
        }
    }

    public class LevelEvolver {
        private List<RoomKind> blueprint;
        private Dictionary<RoomKind, List<RoomData>> roomProviders;

        public LevelEvolver(List<RoomKind> blueprint, Dictionary<RoomKind, List<RoomData>> roomProviders) {
            this.blueprint = blueprint;
            this.roomProviders = roomProviders;
        }

        public List<PlacedRoom> EvolveRoot(Random rng) {
            var evolved = new List<PlacedRoom>();
            var currentId = 0;

            var rootData = blueprint[currentId];

            foreach (var template in roomProviders[rootData].RandomOrderAccess(rng)) {
                evolved.Add(new PlacedRoom(template, Vector2.zero));

                // found a complete level
                if (EvolveNode(ref evolved, rng, currentId)) return evolved;

                // if we could not evolve with the current template, loop and retry
                evolved.RemoveAt(currentId);
            }

            // level is not solvable
            return null;
        }

        private bool EvolveNode(ref List<PlacedRoom> evolved, Random rng, int lastId) {
            var placedRoom = evolved[lastId];

            foreach (var door in placedRoom.Template.doors.RandomOrderAccess(rng)) {
                if (EvolveNodeDoor(ref evolved, rng, lastId, door)) return true;
            }

            return false;
        }

        private bool EvolveNodeDoor(ref List<PlacedRoom> evolved, Random rng, int lastId, DoorData lastDoor) {
            var nextRoomId = lastId + 1;
            var lastPlacedRoom = evolved[lastId];

            // reached end of blueprint branch
            if (nextRoomId >= blueprint.Count) return true;

            var nextRoom = blueprint[nextRoomId];
            var nextTemplates = roomProviders[nextRoom];

            foreach (var template in nextTemplates.RandomOrderAccess(rng)) {
                foreach (var nextDoor in template.doors.RandomOrderAccess(rng)) {
                    // door size mismatch
                    if (nextDoor.size != lastDoor.size) continue;

                    // TODO: would be more efficient to guess the delta instead of trying both?
                    // try door delta positions
                    foreach (var delta in nextDoor.AdjacentDeltas()) {
                        var nextPos = lastPlacedRoom.Position + lastDoor.position + delta - nextDoor.position;
                        var nextPlacedRoom = new PlacedRoom(template, nextPos);

                        // check overlap
                        if (nextPlacedRoom.DoOverlap(lastPlacedRoom) ||
                            evolved.Any(pr => nextPlacedRoom.DoOverlap(pr))) {
                            continue;
                        }

                        evolved.Add(nextPlacedRoom);

                        if (EvolveNode(ref evolved, rng, nextRoomId)) {
                            return true;
                        }

                        evolved.RemoveAt(nextRoomId);
                    }
                }
            }

            return false;
        }
    }
}
