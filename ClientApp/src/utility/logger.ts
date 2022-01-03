export enum LoggerModule {
  SignalR = 1 << 1
}

export class Logger
{
  // Implement dynamic change by config.
  public static moduleFilter = LoggerModule.SignalR;

  public static Log(module: LoggerModule , message: string)
  {
    const value = Logger.moduleFilter & module;
    if (value == module)
    {
      console.log(`[UTB-Client\t| ${LoggerModule[module]}] ${message}`);
    }
  }
}