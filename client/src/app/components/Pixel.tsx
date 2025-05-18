
export class Pixel {

    style = {}
    color = "lime";
    value = 0;

    constructor(type: number, style: object) {
        this.style = style;
        this.value = type;

        switch (type) {
            case 0:
                this.color = "#f3f3f3";
                break;
            case 1:
                this.color = "blue";
                break;
            case 2:
                this.color = "lime";
                break;
            case 3:
                this.color = "yellow";
                break;
            case 4:
                this.color = "purple";
                break;
            case 5:
                this.color = "orange";
                break;
            case 6:
                this.color = "pink";
                break;
            case 7:
                this.color = "brown";
                break;
            case 8:
                this.color = "black";
                break;
            case 9:
                this.color = "gray";
                break;
            default:
                this.color = "#000011";
                break;
        }
    }

    render(key: number) {
        const isEmpty = this.value === 0;

        return (
            <div
                key={key}
                style={{
                    width: "20px",
                    height: "20px",
                    backgroundColor: isEmpty ? "inherit" : this.color,
                    borderRadius: isEmpty ? "0" : "3px",
                    border: isEmpty ? "none" : "none",
                    boxShadow: isEmpty 
                        ? "none" 
                        : `inset 2px 2px 3px rgba(255,255,255,0.4),
                           inset -2px -2px 3px rgba(0,0,0,0.4),
                           0 0 5px ${this.color}`,
                    margin: "1px",
                    transition: "all 0.1s ease",
                    transform: isEmpty ? "scale(1)" : "scale(0.95)",

                    ...this.style,
                }}
            ></div>
        )
    }
}