﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Hubs
{
    [HubName("chat")]
    public class ChatRealtime : Hub
    {
    }
}