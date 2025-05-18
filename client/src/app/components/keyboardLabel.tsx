import styles from "./keyboardLabel.module.css";

export default function KeyboardLabel({
    label,
    description,
    widthMultiplier = 1,
    onClick
}: {
    label: string,
    description?: string,
    widthMultiplier?: number,
    onClick?: () => void,
}) {

    return (
        <div className={styles.container}>
            <div 
            className={`${styles.key}`}
            style={{width: `${widthMultiplier * 100}px`}}
            onClick={onClick}
            role={onClick ? "button" : undefined}
            tabIndex={onClick ? 0 : undefined}
            >
                {label}
                <div className={styles.description}>
                    {description}
                </div>
            </div>
        </div>
    );
}