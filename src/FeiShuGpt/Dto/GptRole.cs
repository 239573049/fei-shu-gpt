namespace FeiShuGpt.Dto;

public enum GptRole
{
    System,

    Assistant,

    User,

    Tool
}

public class GptRoleHelper
{
    public static AuthorRole GetAuthorRole(GptRole role)
    {
        switch (role)
        {
            case GptRole.System:
                return AuthorRole.System;
            case GptRole.Assistant:
                return AuthorRole.Assistant;
            case GptRole.User:
                return AuthorRole.User;
            case GptRole.Tool:
                return AuthorRole.Tool;
        }

        return AuthorRole.System;
    }

    public static GptRole GetGptRole(AuthorRole role)
    {
        if (role == AuthorRole.User)
        {
            return GptRole.User;
        }

        if (role == AuthorRole.System)
        {
            return GptRole.System;
        }

        if (role == AuthorRole.Assistant)
        {
            return GptRole.Assistant;
        }

        return GptRole.Tool;
    }
}