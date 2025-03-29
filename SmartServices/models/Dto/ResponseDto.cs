﻿namespace SmartServices.models.Entity
{
    public class ResponseDto
    {
        public bool IsSuccess { get; set; } = true;
        public object Result { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
