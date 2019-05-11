package results

const (
	RegisterUnknown = "Unknown"
	RegisterOk = "Ok"
	RegisterInvalidRequest = "InvalidRequest"
	RegisterUserExists = "UserExists"
	RegisterIncorrectUsernameFormat = "IncorrectUsernameFormat"
	RegisterIncorrectPasswordFormat = "IncorrectPasswordFormat"
)

type RegisterResult struct {
	Success bool `json:"success"`
	Messages []string `json:"messages"`
}
