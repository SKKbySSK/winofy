package results

const (
	RegisterUnknown = "Unknown"
	RegisterOk = "Ok"
	RegisterInvalidRequest = "InvalidRequest"
	RegisterUserExists = "UserExists"
	RegisterIncorrectUsername = "IncorrectUsername"
	RegisterIncorrectPassword = "IncorrectPassword"
)

type RegisterResult struct {
	Success bool `json:"success"`
	Messages []string `json:"messages"`
}
