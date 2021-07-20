using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace GoofyAlgoTrader.Futures.Core
{
    public class Data : Collection<Bar>
    {
        private Bar _oneMinBar = new Bar();

        public Data()
        {

        }

        private CollectionChange _onChange;

        public event CollectionChange OnChanged
        {
            add
            {
                _onChange += value;
            }
            remove
            {
                _onChange -= value;
            }
        }

        DataSeries _date = new DataSeries(), _time = new DataSeries(), _high = new DataSeries(), _low = new DataSeries(), _open = new DataSeries(), _close = new DataSeries(), _volume = new DataSeries(), _openinterest = new DataSeries();

        #region 数据序列
        /// <summary>
        /// 日期(yyyyMMdd)
        /// </summary>
        public DataSeries D { get => _date; }

        /// <summary>
        /// 时间(0.HHmmss)
        /// </summary>
        public DataSeries T { get => _time; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public DataSeries O { get => _open; }

        /// <summary>
        /// 最高价
        /// </summary>
        public DataSeries H { get => _high; }

        /// <summary>
        /// 最低价
        /// </summary>
        public DataSeries L { get => _low; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public DataSeries C { get => _close; }

        /// <summary>
        /// 成交量
        /// </summary>
        public DataSeries V { get => _volume; }

        /// <summary>
        /// 持仓量
        /// </summary>
        public DataSeries I { get => _openinterest; }

        /// <summary>
        /// 日期(yyyyMMdd)
        /// </summary>
        public DataSeries Date { get => D; }

        /// <summary>
        /// 时间(0.HHmmss)
        /// </summary>
        public DataSeries Time { get => T; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public DataSeries Open { get => O; }

        /// <summary>
        /// 最高价
        /// </summary>
        public DataSeries High { get => H; }

        /// <summary>
        /// 最低价
        /// </summary>
        public DataSeries Low { get => L; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public DataSeries Close { get => C; }

        /// <summary>
        /// 成交量
        /// </summary>
        public DataSeries Volume { get => V; }

        /// <summary>
        /// 持仓量
        /// </summary>
        public DataSeries OpenInterest { get => I; }
        #endregion

        #region 属性
        /// <summary>
        /// 实际行情(无数据时为Instrument== null)
        /// </summary>
        [Description("分笔数据")]
        public Tick Tick { get; set; } = new Tick();

        /// <summary>
        /// 合约信息
        /// </summary>
        [Description("合约信息")]
        public InstrumentInfo InstrumentInfo { get; set; }

        /// <summary>
        /// 最小变动
        /// </summary>
        [Description("最小变动")]
        public double PriceTick { get { return InstrumentInfo.PriceTick; } }

        /// <summary>
        /// 合约
        /// </summary>
        [Description("合约")]
        public string Instrument { get; set; } = string.Empty;

        /// <summary>
        /// 委托合约
        /// </summary>
        [Description("委托合约")]
        public string InstrumentOrder { get; set; } = string.Empty;

        /// <summary>
        /// 周期类型
        /// </summary>
        [Description("周期类型")]
        public EnumIntervalType IntervalType { get; set; } = EnumIntervalType.Min;

        /// <summary>
        /// 周期数
        /// </summary>
        [Description("周期数")]
        public int Interval { get; set; } = 5;

        /// <summary>
        /// 当前K线索引(由左向右从0开始)
        /// </summary>
        [Description("当前K线索引")]
        public int CurrentBar { get => Count == 0 ? -1 : (Count - 1); }

        /// <summary>
        /// 当前的1分钟K线
        /// </summary>
        public Bar CurrentMinBar { get => _oneMinBar; }
        #endregion

        protected override void InsertItem(int index, Bar item)
        {
            base.InsertItem(index, item);
            _date.Add(double.Parse(item.D.ToString("yyyyMMdd")));
            _time.Add(double.Parse(item.D.ToString("0.HHmmss")));

            _open.Add(item.O);
            _high.Add(item.H);
            _low.Add(item.L);
            _volume.Add(item.V);
            _openinterest.Add(item.I);
            _close.Add(item.C);

            _onChange?.Invoke(1, item, item);
        }

        protected override void SetItem(int index, Bar item)
        {
            base.SetItem(index, item);
            Bar old = this[index];
            _high[CurrentBar - index] = item.H;
            _low[CurrentBar - index] = item.L;
            _volume[CurrentBar - index] = item.V;
            _openinterest[CurrentBar - index] = item.I;
            _close[CurrentBar - index] = item.C; 

            _onChange?.Invoke(0, old, item);
        }

        protected override void RemoveItem(int index)
        {
            Bar old = this[index];
            if (_date.Count == Count)
            {
                _date.RemoveAt(index);
                _time.RemoveAt(index);
                _open.RemoveAt(index);
                _high.RemoveAt(index);
                _low.RemoveAt(index);
                _close.RemoveAt(index);
                _volume.RemoveAt(index);
                _openinterest.RemoveAt(index);
            }
            base.RemoveItem(index);
            _onChange?.Invoke(-1, old, old);
        }

        protected override void ClearItems()
        {
            _date.Clear();
            _time.Clear();
            _open.Clear();
            _high.Clear();
            _low.Clear();
            _close.Clear();
            _volume.Clear();
            _openinterest.Clear();
            base.ClearItems();
        }

		/// <summary>
		/// Tick行情
		/// </summary>
		/// <param name="f"></param>
		public void OnTick(Tick f)
		{
			if (this.Instrument != f.InstrumentID)
				return;

			#region 生成or更新K线
			DateTime dt = DateTime.ParseExact(f.UpdateTime, "yyyyMMdd HH:mm:ss", null);
			DateTime dtBegin = dt.Date;
			switch (IntervalType)
			{
				case EnumIntervalType.Sec:
					dtBegin = dtBegin.Date.AddHours(dt.Hour).AddMinutes(dt.Minute).AddSeconds(dt.Second / Interval * Interval);
					break;
				case EnumIntervalType.Min:
					dtBegin = dtBegin.Date.AddHours(dt.Hour).AddMinutes(dt.Minute / Interval * Interval);
					break;
				case EnumIntervalType.Hour:
					dtBegin = dtBegin.Date.AddHours(dt.Hour / Interval * Interval);
					break;
				case EnumIntervalType.Day:
					dtBegin = DateTime.ParseExact(f.TradingDay.ToString(), "yyyyMMdd", null);
					break;
				case EnumIntervalType.Week:
					dtBegin = DateTime.ParseExact(f.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = dtBegin.Date.AddDays(1 - (byte)dtBegin.DayOfWeek);
					break;
				case EnumIntervalType.Month:
					dtBegin = DateTime.ParseExact(f.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
					break;
				case EnumIntervalType.Year:
					dtBegin = DateTime.ParseExact(f.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = new DateTime(dtBegin.Year, 1, 1);
					break;
				default:
					throw new Exception("参数错误");
			}
			if (_oneMinBar == null)
			{
				_oneMinBar = new Bar
				{
					D = DateTime.ParseExact(f.UpdateTime.Substring(0, f.UpdateTime.Length - 3), "yyyyMMdd HH:mm", null),
					TradingDay = f.TradingDay,
					PreVol = f.Volume,
					I = f.OpenInterest,
					V = 0
				};
				_oneMinBar.H = _oneMinBar.L = _oneMinBar.O = _oneMinBar.C = f.LastPrice;
			}
			else
			{
				if (_oneMinBar.D - dt < TimeSpan.FromMinutes(1))
				{
					_oneMinBar.H = Math.Max(_oneMinBar.H, f.LastPrice);
					_oneMinBar.L = Math.Min(_oneMinBar.L, f.LastPrice);
					_oneMinBar.C = f.LastPrice;
					_oneMinBar.V = _oneMinBar.V + f.Volume - _oneMinBar.PreVol;
					_oneMinBar.PreVol = f.Volume;       //逐个tick累加
					_oneMinBar.I = f.OpenInterest;
				}
				else
				{
					_oneMinBar.D = DateTime.ParseExact(f.UpdateTime.Substring(0, f.UpdateTime.Length - 3), "yyyyMMdd HH:mm", null);
					_oneMinBar.TradingDay = f.TradingDay;
					_oneMinBar.I = f.OpenInterest;
					_oneMinBar.V = f.Volume - _oneMinBar.PreVol;
					_oneMinBar.PreVol = f.Volume;
					_oneMinBar.H = _oneMinBar.L = _oneMinBar.O = _oneMinBar.C = f.LastPrice;
				}
			}
			if (Count == 0) //无数据
			{
				Bar bar = new Bar
				{
					D = dtBegin,
					TradingDay = f.TradingDay,
					PreVol = f.Volume,
					I = f.OpenInterest,
					V = 0 // kOld.preVol == 0 ? 0 : _tick.Volume - kOld.preVol;
				};
				bar.H = bar.L = bar.O = bar.C = f.LastPrice;
				Add(bar);
			}
			else
			{
				Bar bar = this[CurrentBar];
				if (bar.D == dtBegin) //在当前K线范围内
				{
					bar.H = Math.Max(bar.H, f.LastPrice);
					bar.L = Math.Min(bar.L, f.LastPrice);
					bar.C = f.LastPrice;
					bar.V = bar.V + f.Volume - bar.PreVol;
					bar.PreVol = f.Volume;      //逐个tick累加
					bar.I = f.OpenInterest;

					this[CurrentBar] = bar; //更新会与 _onChange?.Invoke(0, old, item); 连动
				}
				else if (dtBegin > bar.D)
				{
					Bar di = new Bar
					{
						D = dtBegin,
						TradingDay = f.TradingDay,
						//V = Math.Abs(bar.PreVol - 0) < 1E-06 ? 0 : f.Volume - bar.PreVol,
						V = f.Volume - bar.PreVol,
						PreVol = f.Volume,
						I = f.OpenInterest,
						O = f.LastPrice,
						H = f.LastPrice,
						L = f.LastPrice,
						C = f.LastPrice
					};
					Add(di);
				}
			}
			Tick = f; //更新最后的tick
			#endregion
		}

		/// <summary>
		/// 分钟数据
		/// </summary>
		/// <param name="min"></param>
		internal void OnUpdatePerMin(Bar min)
		{
			_oneMinBar.D = min.D;
			_oneMinBar.C = min.C;
			_oneMinBar.H = min.H;
			_oneMinBar.I = min.I;
			_oneMinBar.L = min.L;
			_oneMinBar.O = min.O;
			_oneMinBar.TradingDay = min.TradingDay;
			_oneMinBar.V = min.V;


			DateTime dtBegin = DateTime.MaxValue;
			switch (IntervalType)
			{
				case EnumIntervalType.Sec:
					dtBegin = min.D.Date.AddHours(min.D.Hour).AddMinutes(min.D.Minute).AddSeconds(min.D.Second / Interval * Interval);
					break;
				case EnumIntervalType.Min:
					dtBegin = min.D.Date.AddHours(min.D.Hour).AddMinutes(min.D.Minute / Interval * Interval);
					break;
				case EnumIntervalType.Hour:
					dtBegin = min.D.Date.AddHours(min.D.Hour / Interval * Interval);
					break;
				case EnumIntervalType.Day:
					dtBegin = DateTime.ParseExact(min.TradingDay.ToString(), "yyyyMMdd", null);
					break;
				case EnumIntervalType.Week:
					dtBegin = DateTime.ParseExact(min.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = dtBegin.Date.AddDays(1 - (byte)dtBegin.DayOfWeek);
					break;
				case EnumIntervalType.Month:
					dtBegin = DateTime.ParseExact(min.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
					break;
				case EnumIntervalType.Year:
					dtBegin = DateTime.ParseExact(min.TradingDay.ToString(), "yyyyMMdd", null);
					dtBegin = new DateTime(dtBegin.Year, 1, 1);
					break;
				default:
					throw new Exception("参数错误");
			}
			if (Count == 0) //无数据
			{
				Bar bar = new Bar
				{
					D = dtBegin,
					PreVol = min.V,
					I = min.I,
					V = min.V, // kOld.preVol == 0 ? 0 : _tick.Volume - kOld.preVol;
				};
				bar.H = min.H;
				bar.L = min.L;
				bar.O = min.O;
				bar.C = min.C;
				Add(bar);
			}
			else
			{
				Bar bar = this[CurrentBar];
				if (bar.D == dtBegin) //在当前K线范围内
				{
					bar.H = Math.Max(bar.H, min.H);
					bar.L = Math.Min(bar.L, min.L);
					bar.V = bar.V + min.V;
					bar.I = min.I;
					bar.C = min.C;

					this[CurrentBar] = bar; //更新会与 _onChange?.Invoke(0, old, item); 连动
				}
				else if (dtBegin > bar.D)
				{
					Bar di = new Bar
					{
						D = dtBegin,
						V = min.V,
						I = min.I,
						O = min.O,
						H = min.H,
						L = min.L,
						C = min.C,
					};
					Add(di);
				}
			}
		}
	}
}
