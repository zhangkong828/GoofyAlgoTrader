using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public enum ResponseStatusType
    {
        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Failed = 0,

        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 1,

        /// <summary>
        /// 服务器异常
        /// </summary>
        [Description("服务器异常")]
        ServerException = 10000,

        /// <summary>
        /// 参数错误
        /// </summary>
        [Description("参数错误")]
        ParameterError = 10001,

        /// <summary>
        /// 无权限操作
        /// </summary>
        [Description("无权限操作")]
        Forbidden = 10002,

        /// <summary>
        /// 刷新Token失败
        /// </summary>
        [Description("刷新Token失败")]
        Unauthorized = 10003,
    }

    public class BaseResponse
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Message { get; set; }

        public BaseResponse()
        {

        }

        public BaseResponse(int code) : this(code, "")
        {

        }

        public BaseResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public void SetBusinessStatus(ResponseStatusType code)
        {
            Code = (int)code;
            Message = GetEnumDescription(code);
        }

        public void SetBusinessStatus(ResponseStatusType code, string message)
        {
            Code = (int)code;
            Message = message;
        }

        public static BaseResponse GetBaseResponse(int code)
        {
            return new BaseResponse(code);
        }

        public static BaseResponse GetBaseResponse(int code, string message)
        {
            return new BaseResponse(code, message);
        }

        public static BaseResponse GetBaseResponse(ResponseStatusType code)
        {
            return new BaseResponse((int)code, GetEnumDescription(code));
        }

        public static BaseResponse GetBaseResponse(ResponseStatusType code, string message)
        {
            return new BaseResponse((int)code, message);
        }

        protected static string GetEnumDescription(Enum enumValue)
        {
            string str = enumValue.ToString();
            System.Reflection.FieldInfo field = enumValue.GetType().GetField(str);
            object[] objs = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (objs.Length == 0) return str;
            System.ComponentModel.DescriptionAttribute da = (System.ComponentModel.DescriptionAttribute)objs[0];
            return da.Description;
        }

        public static BaseResponse GetResponse(bool success)
        {
            return BaseResponse<BaseResponseData>.GetBaseResponse(new BaseResponseData(success));
        }

        public static BaseResponse GetResponse(string msg)
        {
            return BaseResponse<BaseResponseData>.GetBaseResponse(new BaseResponseData(msg));
        }

        public static BaseResponse GetResponse(bool success, string msg)
        {
            return BaseResponse<BaseResponseData>.GetBaseResponse(new BaseResponseData(success, msg));
        }

        public static BaseResponse GetResponse(BaseResponseData data)
        {
            return BaseResponse<BaseResponseData>.GetBaseResponse(data);
        }
    }

    public class BaseResponse<T> : BaseResponse
    {
        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        public BaseResponse()
        {

        }

        public BaseResponse(int code) : base(code)
        {
        }

        public BaseResponse(int code, string message) : base(code, message)
        {
        }

        public BaseResponse(int code, string message, T data)
        {
            Code = code;
            Message = message;
            if (data == null)
            {
                if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
                {
                    Data = Activator.CreateInstance<T>();
                }
                else
                {
                    Data = default(T);
                }
            }
            else
                Data = data;
        }

        public new static BaseResponse<T> GetBaseResponse(int code)
        {
            return new BaseResponse<T>(code);
        }

        public new static BaseResponse<T> GetBaseResponse(int code, string message)
        {
            return new BaseResponse<T>(code, message);
        }

        public static BaseResponse<T> GetBaseResponse(int code, string message, T data)
        {
            return new BaseResponse<T>(code, message, data);
        }

        public static BaseResponse<T> GetBaseResponse(T data)
        {
            var code = ResponseStatusType.Success;
            return new BaseResponse<T>((int)code, GetEnumDescription(code), data);
        }

        public static BaseResponse<T> GetBaseResponse(ResponseStatusType code, T data)
        {
            return new BaseResponse<T>((int)code, GetEnumDescription(code), data);
        }

        public static BaseResponse<T> GetBaseResponse(ResponseStatusType code, string message, T data)
        {
            return new BaseResponse<T>((int)code, message, data);
        }

    }

    public class BaseResponseData
    {
        public BaseResponseData()
        {

        }

        public BaseResponseData(bool success)
        {
            Success = success;
            Message = success ? "成功" : "失败";
        }

        public BaseResponseData(string msg)
        {
            Message = msg;
        }

        public BaseResponseData(bool success, string msg)
        {
            Success = success;
            Message = msg;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
