using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class DbService
    {
        public static void InitDB()
        {
            using (var conn = DbHelper.GetConnection())
            {
                var sql = @"CREATE TABLE If Not Exists `GoofyAlgoTrader`.`Future_Min`  (
								`DateTime` timestamp NOT NULL,
								`Instrument` varchar(32) NOT NULL,
								`Open` decimal NOT NULL,
								`High` decimal NOT NULL,
								`Low` decimal NOT NULL,
								`Close` decimal NOT NULL,
								`Volume` bigint NOT NULL,
								`OpenInterest` decimal NOT NULL,
								`TradingDay` varchar(8) NOT NULL,
								PRIMARY KEY(`DateTime`, `Instrument`),
								INDEX  Future_Min_Instrument_Idx (`Instrument`),
								INDEX  Future_Min_Instrument_TradingDay_Idx (`Instrument`, `TradingDay`),
								INDEX  Future_Min_TradingDay_Idx (`TradingDay`)
							); ";
                conn.Execute(sql);
            }
        }


        public static bool InsertBar(MinBarModel bar)
        {
            using (var conn = DbHelper.GetConnection())
            {
                var sql = "INSERT INTO GoofyAlgoTrader.Future_Min VALUES(@DateTime,@Instrument,@Open,@High,@Low,@Close,@Volume,@OpenInterest,@TradingDay)";
                return conn.Execute(sql, bar) > 0;
            }
        }
    }
}

