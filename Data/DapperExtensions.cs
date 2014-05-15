using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Data
{
	public static class DapperExtensions
	{
		public static IEnumerable<TMain> Map<TMain, TFirstChild, TKey>
			(
			this SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild
			)
		{
			return MapKeys(reader, mainKey, firstChild, addFirstChild);
		}

		public static IEnumerable<TMain> Map<TMain, TFirstChild, TSecondChild, TKey>
			(
			this SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild
			)
		{
			return MapKeys(reader, mainKey, firstChild, secondChild, addFirstChild, addSecondChild);
		}

		public static IEnumerable<TMain> Map<TMain, TFirstChild, TSecondChild, TThirdChild, TKey>
			(
			this SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Func<TThirdChild, TKey> thirdChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild,
			Action<TMain, IEnumerable<TThirdChild>> addThirdChild
			)
		{
			return MapKeys(reader, mainKey, firstChild, secondChild, thirdChild, addFirstChild, addSecondChild, addThirdChild);
		}

		public static IEnumerable<TMain> Map
			<TMain, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild, TSeventhChild, TEighthChild,
			 TKey>
			(
			this SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Func<TThirdChild, TKey> thirdChild,
			Func<TFourthChild, TKey> fourthChild,
			Func<TFifthChild, TKey> fifthChild,
			Func<TSixthChild, TKey> sixthChild,
			Func<TSeventhChild, TKey> seventhChild,
			Func<TEighthChild, TKey> eighthChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild,
			Action<TMain, IEnumerable<TThirdChild>> addThirdChild,
			Action<TMain, IEnumerable<TFourthChild>> addFourthChild,
			Action<TMain, IEnumerable<TFifthChild>> addFifthChild,
			Action<TMain, IEnumerable<TSixthChild>> addSixthChild,
			Action<TMain, IEnumerable<TSeventhChild>> addSeventhChild,
			Action<TMain, IEnumerable<TEighthChild>> addEighthChild
			)
		{
			return MapKeys(reader, mainKey, firstChild, secondChild, thirdChild, fourthChild, fifthChild, sixthChild,
			               seventhChild, eighthChild, addFirstChild, addSecondChild, addThirdChild, addFourthChild, addFifthChild,
			               addSixthChild, addSeventhChild, addEighthChild);
		}

		private static IEnumerable<TMain> MapKeys<TMain, TFirstChild, TKey>
			(
			SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild
			)
		{
			List<TMain> first = reader.Read<TMain>().ToList();
			Dictionary<TKey, IEnumerable<TFirstChild>> childFirstMap = reader
				.Read<TFirstChild>()
				.GroupBy(s => firstChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());

			foreach (TMain item in first)
			{
				IEnumerable<TFirstChild> childrenOfFirst;
				if (childFirstMap.TryGetValue(mainKey(item), out childrenOfFirst))
				{
					addFirstChild(item, childrenOfFirst);
				}
			}

			return first;
		}

		private static IEnumerable<TMain> MapKeys<TMain, TFirstChild, TSecondChild, TKey>
			(
			SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild
			)
		{
			List<TMain> first = reader.Read<TMain>().ToList();
			Dictionary<TKey, IEnumerable<TFirstChild>> childFirstMap = reader
				.Read<TFirstChild>()
				.GroupBy(s => firstChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TSecondChild>> childSecondMap = reader
				.Read<TSecondChild>()
				.GroupBy(s => secondChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());

			foreach (TMain item in first)
			{
				IEnumerable<TFirstChild> childrenOfFirst;
				if (childFirstMap.TryGetValue(mainKey(item), out childrenOfFirst))
				{
					addFirstChild(item, childrenOfFirst);
				}

				IEnumerable<TSecondChild> childrenOfSecond;
				if (childSecondMap.TryGetValue(mainKey(item), out childrenOfSecond))
				{
					addSecondChild(item, childrenOfSecond);
				}
			}

			return first;
		}

		private static IEnumerable<TMain> MapKeys<TMain, TFirstChild, TSecondChild, TThirdChild, TKey>
			(
			SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Func<TThirdChild, TKey> thirdChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild,
			Action<TMain, IEnumerable<TThirdChild>> addThirdChild
			)
		{
			List<TMain> first = reader.Read<TMain>().ToList();
			Dictionary<TKey, IEnumerable<TFirstChild>> childFirstMap = reader
				.Read<TFirstChild>()
				.GroupBy(s => firstChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TSecondChild>> childSecondMap = reader
				.Read<TSecondChild>()
				.GroupBy(s => secondChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TThirdChild>> childThirdMap = reader
				.Read<TThirdChild>()
				.GroupBy(s => thirdChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());

			foreach (TMain item in first)
			{
				IEnumerable<TFirstChild> childrenOfFirst;
				if (childFirstMap.TryGetValue(mainKey(item), out childrenOfFirst))
				{
					addFirstChild(item, childrenOfFirst);
				}

				IEnumerable<TSecondChild> childrenOfSecond;
				if (childSecondMap.TryGetValue(mainKey(item), out childrenOfSecond))
				{
					addSecondChild(item, childrenOfSecond);
				}

				IEnumerable<TThirdChild> childrenOfThird;
				if (childThirdMap.TryGetValue(mainKey(item), out childrenOfThird))
				{
					addThirdChild(item, childrenOfThird);
				}
			}

			return first;
		}

		private static IEnumerable<TMain> MapKeys
			<TMain, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild, TSeventhChild, TEighthChild,
			 TKey>
			(
			SqlMapper.GridReader reader,
			Func<TMain, TKey> mainKey,
			Func<TFirstChild, TKey> firstChild,
			Func<TSecondChild, TKey> secondChild,
			Func<TThirdChild, TKey> thirdChild,
			Func<TFourthChild, TKey> fourthChild,
			Func<TFifthChild, TKey> fifthChild,
			Func<TSixthChild, TKey> sixthChild,
			Func<TSeventhChild, TKey> seventhChild,
			Func<TEighthChild, TKey> eighthChild,
			Action<TMain, IEnumerable<TFirstChild>> addFirstChild,
			Action<TMain, IEnumerable<TSecondChild>> addSecondChild,
			Action<TMain, IEnumerable<TThirdChild>> addThirdChild,
			Action<TMain, IEnumerable<TFourthChild>> addFourthChild,
			Action<TMain, IEnumerable<TFifthChild>> addFifthChild,
			Action<TMain, IEnumerable<TSixthChild>> addSixthChild,
			Action<TMain, IEnumerable<TSeventhChild>> addSeventhChild,
			Action<TMain, IEnumerable<TEighthChild>> addEighthChild
			)
		{
			List<TMain> first = reader.Read<TMain>().ToList();
			Dictionary<TKey, IEnumerable<TFirstChild>> childFirstMap = reader
				.Read<TFirstChild>()
				.GroupBy(s => firstChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TSecondChild>> childSecondMap = reader
				.Read<TSecondChild>()
				.GroupBy(s => secondChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TThirdChild>> childThirdMap = reader
				.Read<TThirdChild>()
				.GroupBy(s => thirdChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TFourthChild>> childFourthMap = reader
				.Read<TFourthChild>()
				.GroupBy(s => fourthChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TFifthChild>> childFifthMap = reader
				.Read<TFifthChild>()
				.GroupBy(s => fifthChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TSixthChild>> childSixthMap = reader
				.Read<TSixthChild>()
				.GroupBy(s => sixthChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TSeventhChild>> childSeventhMap = reader
				.Read<TSeventhChild>()
				.GroupBy(s => seventhChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			Dictionary<TKey, IEnumerable<TEighthChild>> childEighthMap = reader
				.Read<TEighthChild>()
				.GroupBy(s => eighthChild(s))
				.ToDictionary(g => g.Key, g => g.AsEnumerable());

			foreach (TMain item in first)
			{
				IEnumerable<TFirstChild> childrenOfFirst;
				if (childFirstMap.TryGetValue(mainKey(item), out childrenOfFirst))
				{
					addFirstChild(item, childrenOfFirst);
				}

				IEnumerable<TSecondChild> childrenOfSecond;
				if (childSecondMap.TryGetValue(mainKey(item), out childrenOfSecond))
				{
					addSecondChild(item, childrenOfSecond);
				}

				IEnumerable<TThirdChild> childrenOfThird;
				if (childThirdMap.TryGetValue(mainKey(item), out childrenOfThird))
				{
					addThirdChild(item, childrenOfThird);
				}

				IEnumerable<TFourthChild> childrenOfFourth;
				if (childFourthMap.TryGetValue(mainKey(item), out childrenOfFourth))
				{
					addFourthChild(item, childrenOfFourth);
				}

				IEnumerable<TFifthChild> childrenOfFifth;
				if (childFifthMap.TryGetValue(mainKey(item), out childrenOfFifth))
				{
					addFifthChild(item, childrenOfFifth);
				}

				IEnumerable<TSixthChild> childrenOfSixth;
				if (childSixthMap.TryGetValue(mainKey(item), out childrenOfSixth))
				{
					addSixthChild(item, childrenOfSixth);
				}

				IEnumerable<TSeventhChild> childrenOfSeventh;
				if (childSeventhMap.TryGetValue(mainKey(item), out childrenOfSeventh))
				{
					addSeventhChild(item, childrenOfSeventh);
				}

				IEnumerable<TEighthChild> childrenOfEighth;
				if (childEighthMap.TryGetValue(mainKey(item), out childrenOfEighth))
				{
					addEighthChild(item, childrenOfEighth);
				}
			}

			return first;
		}
	}
}