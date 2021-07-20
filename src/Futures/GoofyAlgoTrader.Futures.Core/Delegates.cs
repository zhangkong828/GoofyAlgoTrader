using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures
{
	/// <summary>
	/// 自定义集合变化事件
	/// </summary>
	/// <param name="pType">策略变化:加1;减-1;更新0</param>
	/// <param name="pNew"></param>
	/// <param name="pOld"></param>
	public delegate void CollectionChange(int pType, object pNew, object pOld);
}
